using System.Text.Json.Serialization;

namespace fse.core.models.contentPacks;

public class MapContextTagToItemContentPackItem : BaseContentPackItem
{
	[JsonPropertyName("tag")]
	public string Tag { get; set; } = string.Empty;

	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;
}

