using System.Text.Json.Serialization;

namespace fse.core.models.contentPacks;

public class IgnoreArtisanMappingContentPackItem : BaseContentPackItem
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;
}

