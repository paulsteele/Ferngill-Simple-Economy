using fse.core.models;
using fse.core.models.contentPacks;
using fse.core.services;
using Moq;
using StardewModdingAPI;
using StardewValley.GameData.Machines;

namespace Tests.services;

public class ArtisanServiceTests
{
	private Mock<IMonitor> _mockMonitor;
	private Mock<IModHelper> _mockHelper;
	private Mock<IContentPackService> _mockContentPackService;
	private Mock<IGameContentHelper> _gameContentHelper;
	private ArtisanService _artisanService;
	private EconomyModel _economyModel;

	[SetUp]
	public void Setup()
	{
		_mockMonitor = new Mock<IMonitor>();
		_mockHelper = new Mock<IModHelper>();
		_mockContentPackService = new Mock<IContentPackService>();
		_gameContentHelper = new Mock<IGameContentHelper>();
		_economyModel = new EconomyModel(new Dictionary<int, Dictionary<string, ItemModel>>
			{
				{
					1, new Dictionary<string, ItemModel>
					{
						{ "1", new ItemModel("1") },
						{ "2", new ItemModel("2") },
						{ "3", new ItemModel("3") },
						{ "4", new ItemModel("4") },
						{ "186", new ItemModel("186") },
						{ "184", new ItemModel("184") },
					}
				},
			}
		);
		
		_mockHelper.SetupGet(m => m.GameContent).Returns(_gameContentHelper.Object);

		_artisanService = new ArtisanService(_mockMonitor.Object, _mockHelper.Object, _mockContentPackService.Object);
	}

	private void GenerateMachineData(params (string output, string input)[] mappings)
	{
		var dict = new Dictionary<string, MachineData>
		{
			{
				"machine1", new MachineData
				{
					OutputRules =
					[
						..mappings.Select(m => new MachineOutputRule
						{
							OutputItem = new[]
							{
								new MachineItemOutput { ItemId = m.output },
							}.ToList(),
							Triggers = new[]
							{
								new MachineOutputTriggerRule { RequiredItemId = m.input },
							}.ToList(),
						}).ToArray(),
					],
				}
			},
		};

		_gameContentHelper.Setup(m => m.Load<Dictionary<string, MachineData>>("Data\\Machines")).Returns(dict);
	}

	[Test]
	public void ShouldGenerateBasicMapping()
	{
		GenerateMachineData(("2", "1"));

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("2");

		Assert.That(result.ObjectId, Is.EqualTo("1"));
	}

	[Test]
	public void ShouldHandleNullMappingGracefully()
	{
		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("1");

		Assert.That(result, Is.Null);
	}

	[Test]
	public void ShouldIgnoreNonExistentItems()
	{
		GenerateMachineData(
			("2", "1"),
			("999", "998") // Non-existent items
		);

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("999");

		Assert.That(result, Is.Null);
	}

	[Test]
	public void ShouldHandleMultiStepArtisanChain()
	{
		GenerateMachineData(
			("3", "2"),
			("2", "1")
		);

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("3");

		Assert.That(result.ObjectId, Is.EqualTo("1"));
	}

	[Test]
	public void ShouldBreakCyclesAndLogWarning()
	{
		GenerateMachineData(
			("2", "1"),
			("3", "2"),
			("1", "3") // Creates cycle
		);

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result1 = _artisanService.GetBaseFromArtisanGood("1");
		var result2 = _artisanService.GetBaseFromArtisanGood("2");
		var result3 = _artisanService.GetBaseFromArtisanGood("3");

		Assert.Multiple(() =>
		{
			Assert.That(result1.ObjectId, Is.EqualTo("2"));
			Assert.That(result2, Is.Null);
			Assert.That(result3.ObjectId, Is.EqualTo("2"));
		});

		_mockMonitor.Verify(
			m => m.LogOnce(
				It.Is<string>(s => s.Contains("cycle detected")),
				LogLevel.Warn
			),
			Times.Once
		);
		_mockMonitor.Verify(
			m => m.LogOnce(
				It.Is<string>(s => s.Contains("2 < 1 < 3 < 2")),
				LogLevel.Warn
			),
			Times.Once
		);
	}

	[Test]
	public void ShouldIgnoreSelfReferencingItems()
	{
		GenerateMachineData(("1", "1"));

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("1");

		Assert.That(result, Is.Null);
	}
	
	[Test]
	public void ShouldHandleHardcodedEquivalents()
	{
		GenerateMachineData(
			("186", "184"),
			("1", "186"),
			("2", "184")
		);

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result1 = _artisanService.GetBaseFromArtisanGood("186");
		var result2 = _artisanService.GetBaseFromArtisanGood("2");
		var result3 = _artisanService.GetBaseFromArtisanGood("1");

		Assert.Multiple(() =>
		{
		 Assert.That(result1, Is.Null);
		 Assert.That(result2.ObjectId, Is.EqualTo("184"));
		 Assert.That(result3.ObjectId, Is.EqualTo("184")); // hardcoded equivalent
		});
 }

	[Test]
	public void ShouldHandleEmptyMachineData()
	{
		var dict = new Dictionary<string, MachineData>();

		_gameContentHelper.Setup(m => m.Load<Dictionary<string, MachineData>>("Data\\Machines")).Returns(dict);

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("1");

		Assert.That(result, Is.Null);
	}
	
	[Test]
	public void ShouldHandleIgnoredArtisanMappings()
	{
		GenerateMachineData(
			("2", "1"),
			("3", "1")
		);

		_mockContentPackService.Setup(m => m.GetItemsOfType<IgnoreArtisanMappingContentPackItem>())
			.Returns([
				new IgnoreArtisanMappingContentPackItem { Id = "1" },
			]);

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("2");
		var result2 = _artisanService.GetBaseFromArtisanGood("3");

		Assert.Multiple(() =>
		{
		 Assert.That(result, Is.Null);
		 Assert.That(result2, Is.Null);
		});
 }

	[Test]
	public void ShouldHandleMachineWithNoOutputRules()
	{
		var dict = new Dictionary<string, MachineData>
		{
			{
				"machine1", new MachineData
				{
					OutputRules =
					[
					],
				}
			},
		};

		_gameContentHelper.Setup(m => m.Load<Dictionary<string, MachineData>>("Data\\Machines")).Returns(dict);

		_artisanService.GenerateArtisanMapping(_economyModel);
		var result = _artisanService.GetBaseFromArtisanGood("1");

		Assert.That(result, Is.Null);
	}
}