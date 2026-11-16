using System;
using System.Collections.Generic;
using System.Linq;
using fse.core.models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FruitTrees;
using Object = StardewValley.Object;

namespace fse.core.services;

public interface ISeedService
{
	void GenerateSeedMapping(EconomyModel economyModel);
	ItemModel? GetItemModelFromSeedId(string seed);
	SeedModel? GetSeedModelFromModelId(string modelId);
}

public class SeedService(IMonitor monitor) : ISeedService
{
	private Dictionary<string, ItemModel> SeedToItem { get; } = new();
	private Dictionary<string, SeedModel> ItemToSeed { get; } = new();
	
	public void GenerateSeedMapping(EconomyModel economyModel)
	{
		var cropData = Game1.content.Load<Dictionary<string, CropData>>("Data\\Crops");
		var fruitTreeData = Game1.content.Load<Dictionary<string, FruitTreeData>>("Data\\FruitTrees");
		
		if (cropData == null)
		{
			monitor.LogOnce("Failed generating seed mapping. Missing cropData", LogLevel.Error);
			return;
		}

		if (fruitTreeData == null)
		{
			monitor.LogOnce("Failed generating seed mapping. Missing fruitTreeData", LogLevel.Error);
			return;
		}

		var seeds = cropData
			.Select(kvp => GetSeedModelWithErrors(() => new SeedModel(kvp.Key, kvp.Value)))
			.Concat(fruitTreeData.Select(kvp => GetSeedModelWithErrors(() => new SeedModel(kvp.Key, kvp.Value))))
			.OfType<SeedModel>();

		foreach (var seedModel in seeds)
		{
			try
			{
				var obj = new Object(seedModel.CropId, 1);
				if (!economyModel.CategoryEconomies.TryGetValue(obj.Category, out var category))
				{
					continue;
				}

				if (!category.TryGetValue(seedModel.CropId, out var model))
				{
					continue;
				}

				SeedToItem.TryAdd(seedModel.ObjectId, model);
				ItemToSeed.TryAdd(model.ObjectId, seedModel);
			}
			catch(Exception ex)
			{
				monitor.LogOnce("Failed generating seed mapping.", LogLevel.Error);
				monitor.LogOnce(ex.ToString(), LogLevel.Error);
			}
		}
	}

	private SeedModel? GetSeedModelWithErrors(Func<SeedModel> getter)
	{
		try
		{
			return getter();
		}
		catch (Exception ex)
		{
			monitor.LogOnce("Failed generating seed mapping.", LogLevel.Error);
			monitor.LogOnce(ex.ToString(), LogLevel.Error);
			return null;
		}
	}
	
	public ItemModel? GetItemModelFromSeedId(string seed) => SeedToItem.GetValueOrDefault(seed);
	public SeedModel? GetSeedModelFromModelId(string modelId) => ItemToSeed.GetValueOrDefault(modelId);
}