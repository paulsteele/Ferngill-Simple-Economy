using System.Text.Json.Serialization;

namespace fse.core.models.contentPacks;

public class MapEquivalentItemsContentPackItem : BaseContentPackItem
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	[JsonPropertyName("base")]
	public string Base { get; init; } = string.Empty;
}

