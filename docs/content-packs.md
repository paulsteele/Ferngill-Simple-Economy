See https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Content_Packs & https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest for more information.

To create a content pack for Ferngill Simple Economy, follow these steps:
1. Make a new folder
2. Create a `manifest.json` file in that folder with the following structure:

```json
{
	"Name": "Your Project Name",
	"Author": "your name",
	"Version": "1.0.0",
	"Description": "One or two sentences about the mod.",
	"UniqueID": "YourName.YourProjectName",
	"UpdateKeys": [],
	"ContentPackFor": {
		"UniqueID": "paulsteele.fse"
	}
}
```

3. Create a `content.json` file in that folder than is an *array* of modification objects. 

```
[
	{"action": "<action-name>", <other-properties>},
	...
]
```

where the valid modifications are specified below:

| action                  | description                                                                                                                                                      | properties                                                                                                                                                                                       | example                                                                                                      |
|-------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------|
| `IgnoreArtisanMapping`  | Prevents an item from being treated as a base item for artisan goods, but leaves the item in the economy.                                                        | `id` (string, required): The item ID to ignore                                                                                                                                                   | `{"action": "IgnoreArtisanMapping", "id": "289"}`                                                            |
| `IgnoreInEconomy`       | Excludes an item from economy calculations entirely.                                                                                                             | `id` (string, required): The item ID to exclude                                                                                                                                                  | `{"action": "IgnoreInEconomy", "id": "74"}`                                                                  |
| `MapContextTagToItem`   | Maps a context tag to a specific item for economy grouping. This means any machine recipe that uses a context tag for a recipe will treat this item as that tag. | `tag` (string, required): The context tag<br>`id` (string, required): The item ID to map to                                                                                                      | `{"action": "MapContextTagToItem", "tag": "egg_item", "id": "176"}`                                          |
| `MapEquivalentItems`    | Links two item's supply and daily delta together.                                                                                                                | `id` (string, required): The item ID to link<br>`base` (string, required): The base item ID                                                                                                      | `{"action": "MapEquivalentItems", "id": "174", "base": "176"}`                                               |
| `MapItemToSeason`       | Assigns seasonal availability to an item.                                                                                                                        | `id` (string, required): The item ID<br>`spring` (bool): Available in spring<br>`summer` (bool): Available in summer<br>`fall` (bool): Available in fall<br>`winter` (bool): Available in winter | `{"action": "MapItemToSeason", "id": "16", "spring": true, "summer": false, "fall": false, "winter": false}` |

**Note** FSE does not attempt to resolve any conflicts between packs.