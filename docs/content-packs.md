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

| action | description | properties | example |
|--------|-------------|------------|---------|
