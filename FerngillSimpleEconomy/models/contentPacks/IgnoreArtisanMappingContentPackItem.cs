using System.Text.Json;
using System.Text.Json.Serialization;

namespace fse.core.models.contentPacks;

public class IgnoreArtisanMappingContentPackItem : BaseContentPackItem, IJsonOnDeserialized
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	public void OnDeserialized()
	{
		if (Id == string.Empty)
		{
			throw new JsonException("IgnoreArtisanMapping requires a non-empty 'id' field.");
		}
	}
}

