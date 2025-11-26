using System;
using System.Collections.Generic;
using System.Linq;
using fse.core.models.contentPacks;
using StardewModdingAPI;

namespace fse.core.services;

public interface IContentPackService
{
	void LoadContentPacks();
	IEnumerable<T> GetItemsOfType<T>() where T : BaseContentPackItem;
}

public class ContentPackService(IMonitor monitor, IModHelper helper) : IContentPackService
{
	private readonly List<BaseContentPackItem> _loadedItems = [];

	public void LoadContentPacks()
	{
		var contentPacks = helper.ContentPacks.GetOwned().ToArray();
		
		if (contentPacks.Length == 0)
		{
			monitor.Log("No content packs found.", LogLevel.Info);
			return;
		}
		
		foreach (var contentPack in contentPacks)
		{
			try
			{
				var items = contentPack.ReadJsonFile<List<BaseContentPackItem>>("content.json") ?? [];
				
				_loadedItems.AddRange(items);
				
				monitor.Log($"Loaded {items.Count} item(s) from content pack {contentPack.Manifest.Name}", LogLevel.Info);
			}
			catch (Exception ex)
			{
				monitor.Log($"Error loading content pack {contentPack.Manifest.Name}: {ex.Message}", LogLevel.Error);
			}
		}
	}

	public IEnumerable<T> GetItemsOfType<T>() where T : BaseContentPackItem => _loadedItems.OfType<T>();
}