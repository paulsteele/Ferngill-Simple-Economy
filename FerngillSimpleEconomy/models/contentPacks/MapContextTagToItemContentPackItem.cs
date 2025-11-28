using System.Text.Json;
using System.Text.Json.Serialization;

namespace fse.core.models.contentPacks;

public class MapContextTagToItemContentPackItem : BaseContentPackItem, IJsonOnDeserialized
{
	[JsonPropertyName("tag")]
	public string Tag { get; init; } = string.Empty;

	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	public void OnDeserialized()
	{
		if (Tag == string.Empty)
		{
			throw new JsonException("MapContextTagToItem requires a non-empty 'tag' field.");
		}

		if (Id == string.Empty)
		{
			throw new JsonException("MapContextTagToItem requires a non-empty 'id' field.");
		}
	}
}

