using fse.core.models.contentPacks;
using fse.core.services;
using Moq;
using StardewModdingAPI;

namespace Tests.services;

public class ContentPackServiceTests
{
	private Mock<IMonitor> _mockMonitor;
	private Mock<IModHelper> _mockHelper;
	private Mock<IContentPackHelper> _mockContentPackHelper;
	private ContentPackService _contentPackService;
	private Mock<IFileService> _fileService;

	[SetUp]
	public void Setup()
	{
		_mockMonitor = new Mock<IMonitor>();
		_mockHelper = new Mock<IModHelper>();
		_fileService = new Mock<IFileService>();
		_mockContentPackHelper = new Mock<IContentPackHelper>();

		_mockHelper.SetupGet(m => m.ContentPacks)
			.Returns(_mockContentPackHelper.Object);

		_contentPackService = new ContentPackService(_mockMonitor.Object, _mockHelper.Object, _fileService.Object);
	}

	[Test]
	public void ShouldDeserializeMixedContentPacks([Values] bool splitPacks)
	{
		var lines = new List<(string text, bool isValid)>
		{
			("""{"action": "IgnoreArtisanMapping", "id": "item1", "comment": "extra field"}""", true),
			("""{"action": "MapContextTagToItem", "tag": "tag2", "id": "item2"}""", true),
			("""{"action": "Bad"}""", false),
			("""{"action": "IgnoreArtisanMapping", "bad": "bad1"}""", false),
			("""{"action": "MapEquivalentItems", "id": "item3", "base": "base3"}""", true),
			("""{"action": "IgnoreInEconomy", "id": "item4" }""", true),
			("{}", false),
			("""{"something": "not good", "id": "no" }""", false),
			("""{"action": "MapItemToSeason", "id": "item5", "spring": true, "summer": false, "fall": true, "winter": false}""", true),
		};

		if (splitPacks)
		{
			var packs = lines.Select((line, index) =>
				CreateMockContentPack(
					$"pack{index + 1}",
					new SemanticVersion(1, 0, index + 1),
					"content.json",
					$"[{line.text}]"
				)
			).ToArray();
			
			_mockContentPackHelper.Setup(cp => cp.GetOwned()).Returns(packs);
		}
		else
		{
			var pack = CreateMockContentPack(
				"pack1",
				new SemanticVersion(1, 1, 1),
				"content.json",
				$"[{string.Join(",", lines.Select(l => l.text))}]"
			);

			_mockContentPackHelper.Setup(cp => cp.GetOwned()).Returns([pack]);
		}

		ReInitialize();

		var ignoreArtisanMappingContentPackItems = _contentPackService.GetItemsOfType<IgnoreArtisanMappingContentPackItem>().ToArray();
		var mapContextTagToItemContentPackItems = _contentPackService.GetItemsOfType<MapContextTagToItemContentPackItem>().ToArray();
		var mapEquivalentItemsContentPackItems = _contentPackService.GetItemsOfType<MapEquivalentItemsContentPackItem>().ToArray();
		var ignoreInEconomyContentPackItems = _contentPackService.GetItemsOfType<IgnoreInEconomyContentPackItem>().ToArray();
		var packInfo = _contentPackService.GetContentPackInfo().ToArray();

		Assert.Multiple(() =>
		{
			Assert.That(ignoreArtisanMappingContentPackItems, Has.Length.EqualTo(1));
			Assert.That(ignoreArtisanMappingContentPackItems[0].Id, Is.EqualTo("item1"));
			
			Assert.That(mapContextTagToItemContentPackItems, Has.Length.EqualTo(1));
			Assert.That(mapContextTagToItemContentPackItems[0].Id, Is.EqualTo("item2"));
			Assert.That(mapContextTagToItemContentPackItems[0].Tag, Is.EqualTo("tag2"));
			
			Assert.That(mapEquivalentItemsContentPackItems, Has.Length.EqualTo(1));
			Assert.That(mapEquivalentItemsContentPackItems[0].Id, Is.EqualTo("item3"));
			Assert.That(mapEquivalentItemsContentPackItems[0].Base, Is.EqualTo("base3"));
			
			Assert.That(ignoreInEconomyContentPackItems, Has.Length.EqualTo(1));
			Assert.That(ignoreInEconomyContentPackItems[0].Id, Is.EqualTo("item4"));
	
			Assert.That(packInfo, Has.Length.EqualTo(splitPacks ? lines.Count : 1));
			if (splitPacks)
			{
				for (var i = 0; i < lines.Count; i++)
				{
					Assert.That(packInfo[i].Name, Is.EqualTo($"pack{i + 1} v1.0.{i + 1}"));
					Assert.That(packInfo[i].LoadedItemsCount, Is.EqualTo(lines[i].isValid ? 1 : 0));
				}
			}
			else
			{
				Assert.That(packInfo[0].Name, Is.EqualTo("pack1 v1.1.1"));
				Assert.That(packInfo[0].LoadedItemsCount, Is.EqualTo(lines.Count(l => l.isValid)));
			}
		});
	}
	
	[Test]
	public void ShouldHandleNoContentPacks()
	{
		_mockContentPackHelper.Setup(cp => cp.GetOwned()).Returns([]);

		ReInitialize();

		var allItems = _contentPackService.GetItemsOfType<BaseContentPackItem>().ToArray();

		Assert.That(allItems, Is.Empty);
	}

	[Test]
	public void ShouldHandleContentPackWithNoContentFile()
	{
		var pack = CreateMockContentPack(
			"pack1",
			new SemanticVersion(1, 1, 1),
			"misspelled.json",
			"""[{"action": "IgnoreArtisanMapping", "id": "item1", "comment": "extra field"}]"""
		);
		
		_mockContentPackHelper.Setup(cp => cp.GetOwned()).Returns([pack]);

		ReInitialize();

		var allItems = _contentPackService.GetItemsOfType<BaseContentPackItem>().ToArray();

		Assert.That(allItems, Is.Empty);
	}

	[Test]
	public void ShouldHandleBadJson()
	{
		var pack = CreateMockContentPack(
			"pack1",
			new SemanticVersion(1, 1, 1),
			"content.json",
			"""{"action": "IgnoreArtisanMapping", "id": "item1", "comment": "extra field"}"""
		);
		
		_mockContentPackHelper.Setup(cp => cp.GetOwned()).Returns([pack]);

		ReInitialize();

		var allItems = _contentPackService.GetItemsOfType<BaseContentPackItem>().ToArray();

		Assert.That(allItems, Is.Empty);
	}

	private IContentPack CreateMockContentPack(string name, SemanticVersion version, string contentName, string content)
	{
		var directoryPath = $"/fake/path/{name}";
		
		var manifest = new Mock<IManifest>();
		manifest.SetupGet(m => m.Name).Returns(name);
		manifest.SetupGet(m => m.Version).Returns(version);
		
		var pack = new Mock<IContentPack>();
		pack.SetupGet(m => m.Manifest).Returns(manifest.Object);
		pack.SetupGet(m => m.DirectoryPath).Returns(directoryPath);
		
		_fileService.Setup(fs => fs.ReadAllText(Path.Combine(directoryPath, contentName)))
			.Returns(content);

		return pack.Object;
	}

	private void ReInitialize()
	{
		_contentPackService = new ContentPackService(_mockMonitor.Object, _mockHelper.Object, _fileService.Object);
	}
}