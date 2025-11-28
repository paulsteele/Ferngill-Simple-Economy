using System.Text.Json;
using System.Text.Json.Serialization;

namespace fse.core.models.contentPacks;

public class MapItemToSeasonContentPackItem : BaseContentPackItem, IJsonOnDeserialized
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = string.Empty;

	[JsonPropertyName("spring")]
	public bool Spring { get; init; }

	[JsonPropertyName("summer")]
	public bool Summer { get; init; }

	[JsonPropertyName("fall")]
	public bool Fall { get; init; }

	[JsonPropertyName("winter")]
	public bool Winter { get; init; }

	[JsonIgnore]
	public Seasons Seasons
	{
		get
		{
			var seasons = (Seasons)0;
			if (Spring)
			{
				seasons |= Seasons.Spring;
			}
			if (Summer)
			{
				seasons |= Seasons.Summer;
			}
			if (Fall)
			{
				seasons |= Seasons.Fall;
			}
			if (Winter)
			{
				seasons |= Seasons.Winter;
			}
			return seasons;
		}
	}

	public void OnDeserialized()
	{
		if (Id == string.Empty)
		{
			throw new JsonException("MapItemToSeason requires a non-empty 'id' field.");
		}
	}
}

