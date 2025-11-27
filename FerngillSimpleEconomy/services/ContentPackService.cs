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

public class ContentPackService(IMonitor monitor, IModHelper helper, IFileService fileService) : IContentPackService
{
	private const string ContentFileName = "content.json";
	private readonly List<BaseContentPackItem> _loadedItems = [];

	public void LoadContentPacks()
	{
		var contentPacks = helper.ContentPacks.GetOwned().ToArray();
		
		if (contentPacks.Length == 0)
		{
			monitor.Log("No content packs found.");
			return;
		}
		
		foreach (var contentPack in contentPacks)
		{
			var packString = $"{contentPack.Manifest.Name} v{contentPack.Manifest.Version}";
			try
			{
				var filePath = Path.Combine(contentPack.DirectoryPath, ContentFileName);
				
				var jsonText = fileService.ReadAllText(filePath);
				if (jsonText == null)
				{
					monitor.Log($"{packString} has no {ContentFileName}.", LogLevel.Warn);
					continue;
				}

				var items = JsonSerializer.Deserialize<List<BaseContentPackItem>>(jsonText) ?? [];

				var groupedItems = items
					.ToLookup(g => g.GetType() != typeof(InvalidContentPackItem), g => g);
				
				foreach (var invalid in groupedItems[false].OfType<InvalidContentPackItem>())
				{
					monitor.Log(
						$"Invalid entry in {packString} skipped: error='{invalid.Exception?.Message}, text={invalid.RawText}'",
						LogLevel.Error
					);
				}
				
				_loadedItems.AddRange(groupedItems[true]);
				
				monitor.Log($"Loaded {items.Count} item(s) from {packString}");
			}
			catch (Exception ex)
			{
				monitor.Log($"Error loading {packString}: {ex.Message}", LogLevel.Error);
			}
		}
	}

	public IEnumerable<T> GetItemsOfType<T>() where T : BaseContentPackItem => _loadedItems.OfType<T>();
}