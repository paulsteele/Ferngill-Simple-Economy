using System.Text.Json.Serialization;
using fse.core.helpers;

namespace fse.core.models.contentPacks;

[JsonConverter(typeof(ContentPackItemJsonConverter))]
public abstract class BaseContentPackItem
{
	[JsonPropertyName("action")]
	public string Action { get; set; } = string.Empty;
}

