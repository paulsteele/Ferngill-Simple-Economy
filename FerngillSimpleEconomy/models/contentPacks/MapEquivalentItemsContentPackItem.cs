using System.Text.Json;
using System.Text.Json.Serialization;

namespace fse.core.models.contentPacks;

public class MapEquivalentItemsContentPackItem : BaseContentPackItem, IJsonOnDeserialized
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	[JsonPropertyName("base")]
	public string Base { get; init; } = string.Empty;

	public void OnDeserialized()
	{
		if (Id == string.Empty)
		{
			throw new JsonException("MapEquivalentItems requires a non-empty 'id' field.");
		}

		if (Base == string.Empty)
		{
			throw new JsonException("MapEquivalentItems requires a non-empty 'base' field.");
		}
	}
}

