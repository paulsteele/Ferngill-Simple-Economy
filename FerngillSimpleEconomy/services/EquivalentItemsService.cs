using System.Collections.Generic;
using System.Linq;
using fse.core.models.contentPacks;

namespace fse.core.services;

public interface IEquivalentItemsService
{
	public string ResolveEquivalentId(string id);
}

public class EquivalentItemsService : IEquivalentItemsService
{
	private readonly Dictionary<string, string> _equivalentItems;
	
	public EquivalentItemsService(IContentPackService contentPackService)
	{
		_equivalentItems = contentPackService.GetItemsOfType<MapEquivalentItemsContentPackItem>()
			.ToDictionary(i => i.Id, i => i.Base);
	}

	public string ResolveEquivalentId(string id) => _equivalentItems.GetValueOrDefault(id, id);
}