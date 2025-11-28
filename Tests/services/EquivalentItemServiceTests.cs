using fse.core.models.contentPacks;
using fse.core.services;
using Moq;

namespace Tests.services;

public class EquivalentItemServiceTests
{
	private Mock<IContentPackService> _mockContentPackService;
	
	private EquivalentItemsService _equivalentItemsService;

	[SetUp]
	public void Setup()
	{
		_mockContentPackService = new Mock<IContentPackService>();

		_mockContentPackService.Setup(m => m.GetItemsOfType<MapEquivalentItemsContentPackItem>())
			.Returns([
				new MapEquivalentItemsContentPackItem { Id = "eq1", Base = "base1" },
				new MapEquivalentItemsContentPackItem { Id = "eq2", Base = "base2" },
				new MapEquivalentItemsContentPackItem { Id = "eq3", Base = "eq2" }, // The service does not support multi-level mapping
			]);
		
		_equivalentItemsService = new EquivalentItemsService(_mockContentPackService.Object);
	}

	[TestCase("eq1", ExpectedResult = "base1")]
	[TestCase("eq2", ExpectedResult = "base2")]
	[TestCase("eq3", ExpectedResult = "eq2")]
	[TestCase("base1", ExpectedResult = "base1")]
	[TestCase("unknown", ExpectedResult = "unknown")]
	public string ShouldResolveEquivalentId(string id) => _equivalentItemsService.ResolveEquivalentId(id);
}