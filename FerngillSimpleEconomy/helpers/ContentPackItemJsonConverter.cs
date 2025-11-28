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

		var rawText = root.GetRawText();

		try
		{
			if (!root.TryGetProperty(DiscriminatorField, out var actionProperty))
			{
				throw new JsonException($"Missing '{DiscriminatorField}' property in ContentPackItem");
			}

			var action = actionProperty.GetString();
			
			BaseContentPackItem? returnValue = action switch
			{
				"IgnoreArtisanMapping" => JsonSerializer.Deserialize<IgnoreArtisanMappingContentPackItem>(rawText, options),
				"MapContextTagToItem" => JsonSerializer.Deserialize<MapContextTagToItemContentPackItem>(rawText, options),
				"MapEquivalentItems" => JsonSerializer.Deserialize<MapEquivalentItemsContentPackItem>(rawText, options),
				"IgnoreInEconomy" => JsonSerializer.Deserialize<IgnoreInEconomyContentPackItem>(rawText, options),
				"MapItemToSeason" => JsonSerializer.Deserialize<MapItemToSeasonContentPackItem>(rawText, options),
				_ => throw new JsonException($"Unknown action type: {action}"),
			};

			// ReSharper disable once ConvertIfStatementToNullCoalescingExpression
			if (returnValue == null)
			{
				returnValue = new InvalidContentPackItem { RawText = rawText };
			}

			return returnValue;
		}
		catch (Exception e)
		{
			return new InvalidContentPackItem { RawText = rawText, Exception = e};
		}
	}

	public override void Write(Utf8JsonWriter writer, BaseContentPackItem value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, value.GetType(), options);
	}
}

