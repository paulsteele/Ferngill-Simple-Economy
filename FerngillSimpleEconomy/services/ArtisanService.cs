using System.Collections.Generic;
using System.Linq;
using fse.core.models;
using fse.core.models.contentPacks;
using StardewModdingAPI;
using StardewValley.GameData.Machines;

namespace fse.core.services;

public interface IArtisanService
{
	void GenerateArtisanMapping(EconomyModel economyModel);
	ItemModel? GetBaseFromArtisanGood(string modelId);
}

public class ArtisanService(
	IMonitor monitor,
	IModHelper helper,
	IContentPackService contentPackService,
	IEquivalentItemsService equivalentItemsService
) : IArtisanService
{
	private const string ObjectPrefix = "(O)";
	private Dictionary<string, string>? _artisanGoodToBase;
	private EconomyModel? _economyModel;
	
	public void GenerateArtisanMapping(EconomyModel economyModel)
	{
		if (ConfigModel.Instance.DisableArtisanMapping)
		{
			monitor.LogOnce("Artisan Good Mapping disabled by config", LogLevel.Info);
			_economyModel = economyModel;
			_artisanGoodToBase = new Dictionary<string, string>();
			return;
		}
		
		var machineData = helper.GameContent.Load<Dictionary<string, MachineData>>("Data\\Machines");

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (machineData == null)
		{
			return;
		}

		var artisanMappingIgnoreList = contentPackService.GetItemsOfType<IgnoreArtisanMappingContentPackItem>()
			.Select(i => i.Id)
			.ToArray();
		
		var contextTagItemMapping = contentPackService.GetItemsOfType<MapContextTagToItemContentPackItem>()
			.ToDictionary(i => i.Tag, i => i.Id);

		_artisanGoodToBase = machineData.Values
			.Where(m => m.OutputRules != null)
			.SelectMany(m => m.OutputRules)
			.Where(r => r.OutputItem != null && r.Triggers != null)
			.SelectMany(r => r.OutputItem.SelectMany(output => r.Triggers.Select(trigger => (output, trigger))))
			.Select(t =>
			{
				t.trigger.RequiredItemId ??= t.trigger.RequiredTags?.Select(tag => contextTagItemMapping.GetValueOrDefault(tag.Replace(ObjectPrefix, string.Empty)))
					.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item));
				return t;
			})
			.Where(t => !string.IsNullOrWhiteSpace(t.trigger.RequiredItemId))
			.Where(t => !string.IsNullOrWhiteSpace(t.output.ItemId))
			.Select(t => (
				output: t.output.ItemId.Replace(ObjectPrefix, string.Empty),
				input: t.trigger.RequiredItemId.Replace(ObjectPrefix, string.Empty))
			)
			.Where(t => !equivalentItemsService.ResolveEquivalentId(t.input).Equals(equivalentItemsService.ResolveEquivalentId(t.output)))
			.Where(t => !artisanMappingIgnoreList.Contains(t.input))
			.Where(t => economyModel.HasItem(equivalentItemsService, t.output) && economyModel.HasItem(equivalentItemsService, t.input))
			.GroupBy(t => t.output)
			.Select(g =>
			{
				var first = g.First();
				return (first.output, first.input);
			})
			.ToDictionary(t => t.output, t => t.input);

		monitor.LogOnce("Artisan Good Mapping Trace");
		foreach (var mapping in _artisanGoodToBase)
		{
			monitor.LogOnce($"{mapping.Key} based on {mapping.Value}");
		}
		
		_economyModel = economyModel;
		
		BreakCycles();
	}

	public ItemModel? GetBaseFromArtisanGood(string modelId)
	{
		if (_economyModel == null || _artisanGoodToBase == null)
		{
			return null;
		}

		if (!_artisanGoodToBase.ContainsKey(modelId))
		{
			return null;
		}

		var id = modelId;

		while (_artisanGoodToBase.TryGetValue(id, out var mappedItem))
		{
			id = mappedItem;
		}

		return _economyModel.GetItem(equivalentItemsService, id);
	}

	private void BreakCycles()
	{
		if (_artisanGoodToBase == null)
		{
			return;
		}

		var keys = _artisanGoodToBase.Keys.ToArray();

		foreach (var key in keys)
		{
			var id = key;
			var seen = new HashSet<string>{id};
			
			while (_artisanGoodToBase.TryGetValue(id, out var mappedItem))
			{
				if (!seen.Add(mappedItem))
				{
					var path = string.Join(" < ", seen) + " < " + mappedItem;
					monitor.LogOnce($"Artisan good cycle detected for {key}. Item economy will be unique. This may be intended by the mod author. Please report an issue if the following chain looks suspect.\n{path}", LogLevel.Warn);
					_artisanGoodToBase.Remove(key);
					break;
				}

				id = mappedItem;
			}
		}
	}
}
