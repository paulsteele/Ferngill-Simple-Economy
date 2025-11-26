using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using fse.core.models.contentPacks;
using StardewModdingAPI;

namespace fse.core.services;

public interface IContentPackService
{
	IEnumerable<T> GetItemsOfType<T>() where T : BaseContentPackItem;
}

public class ContentPackService(IMonitor monitor, IModHelper helper) : IContentPackService
{
	private const string ContentFileName = "content.json";
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
			var packString = $"{contentPack.Manifest.Name} v{contentPack.Manifest.Version}";
			try
			{
				var filePath = Path.Combine(contentPack.DirectoryPath, ContentFileName);
				
				if (!File.Exists(filePath))
				{
					monitor.Log($"{packString} has no {ContentFileName}.", LogLevel.Warn);
					continue;
				}
				
				var jsonText = File.ReadAllText(filePath);
				
				var items = JsonSerializer.Deserialize<List<BaseContentPackItem>>(jsonText) ?? [];
				
				_loadedItems.AddRange(items);
				
				monitor.Log($"Loaded {items.Count} item(s) from {packString}", LogLevel.Info);
			}
			catch (Exception ex)
			{
				monitor.Log($"Error loading {packString}: {ex.Message}", LogLevel.Error);
			}
		}
	}

	public IEnumerable<T> GetItemsOfType<T>() where T : BaseContentPackItem => _loadedItems.OfType<T>();
}