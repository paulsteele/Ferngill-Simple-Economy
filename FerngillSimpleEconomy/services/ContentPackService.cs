using System.Linq;
using StardewModdingAPI;

namespace fse.core.services;

public interface IContentPackService
{
	void LoadContentPacks();
}

public class ContentPackService(IMonitor monitor, IModHelper helper) : IContentPackService
{
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
			monitor.Log($"Found content pack: {contentPack.Manifest.Name} v{contentPack.Manifest.Version}", LogLevel.Info);
		}
	}
}