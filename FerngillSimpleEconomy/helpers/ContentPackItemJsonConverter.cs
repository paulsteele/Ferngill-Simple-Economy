using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using fse.core.models.contentPacks;

namespace fse.core.helpers;

public class ContentPackItemJsonConverter : JsonConverter<BaseContentPackItem>
{
	private const string DiscriminatorField = "action";

	public override BaseContentPackItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		if (!root.TryGetProperty(DiscriminatorField, out var actionProperty))
		{
			throw new JsonException($"Missing '{DiscriminatorField}' property in ContentPackItem");
		}

		var action = actionProperty.GetString();

		return action switch
		{
			"IgnoreArtisanMapping" => JsonSerializer.Deserialize<IgnoreArtisanMappingContentPackItem>(root.GetRawText(), options),
			_ => throw new JsonException($"Unknown action type: {action}"),
		};
	}

	public override void Write(Utf8JsonWriter writer, BaseContentPackItem value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, value.GetType(), options);
	}
}

