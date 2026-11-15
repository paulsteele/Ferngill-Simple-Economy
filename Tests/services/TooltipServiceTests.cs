using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using StardewValley;
using Object = StardewValley.Object;

namespace Tests.services;

public class TooltipServiceTests : HarmonyTestBase
{
	private Mock<IEconomyService> _mockEconomyService;
	private Mock<IDrawSupplyBarHelper> _mockDrawSupplyBarHelper;

	private TooltipService _tooltipService;

	private Object _validObject;
	private Object _invalidObject;

	[SetUp]
	public override void Setup()
	{
		_mockEconomyService = new Mock<IEconomyService>();
		_mockDrawSupplyBarHelper = new Mock<IDrawSupplyBarHelper>();
	
		_validObject = new Object("1", 1);
		_invalidObject = new Object("2", 1);

		_mockEconomyService.Setup(m => m.GetItemModelFromObject(_validObject)).Returns(new ItemModel("1"));

		_tooltipService = new TooltipService(_mockEconomyService.Object, _mockDrawSupplyBarHelper.Object);
	}

	[TearDown]
	public override void TearDown()
	{
		ConfigModel.Instance.EnableTooltip = true;
	}
	
	[TestCase(true, true, 0, 0, 0, 0, 110)]
	[TestCase(true, true, 200, 0, 0, 0, 310)]
	[TestCase(true, true, 300, 0, 0, 0, 410)]
	[TestCase(true, true, 300, 0, 100, 0, 210)]
	[TestCase(true, true, 300, 100, 0, 100, 410)]
	[TestCase(false, true, 200, 0, 0, 0, 0)]
	[TestCase(false, true, 300, 0, 0, 0, 0)]
	[TestCase(false, true, 300, 0, 100, 0, 100)]
	[TestCase(false, true, 300, 100, 0, 100, 0)]
	[TestCase(true, false, 0, 0, 0, 0, 0)]
	[TestCase(true, false, 200, 0, 0, 0, 0)]
	[TestCase(true, false, 300, 0, 0, 0, 0)]
	[TestCase(true, false, 300, 0, 100, 0, 100)]
	[TestCase(true, false, 300, 100, 0, 100, 0)]
	public void ShouldGetExtraSpaceNeededForTooltipSpecialIcons(
		bool tooltipEnabled,
		bool validItem,
		int startingHeight,
		int x,
		int y,
		int expectedX,
		int expectedY
	)
	{
		ConfigModel.Instance.EnableTooltip = tooltipEnabled;
		var result = _tooltipService.GetExtraSpaceNeededForTooltipSpecialIcons(validItem ? _validObject : _invalidObject, startingHeight, new Point(x, y));
		Assert.That(result, Is.EqualTo(new Point(expectedX, expectedY)));
	}

	[TestCase(true, true, 0, 0, 0, 110)]
	[TestCase(true, true, 0, 0, 0, 310)]
	[TestCase(true, true, 0, 0, 0, 410)]
	[TestCase(true, true, 0, 100, 0, 210)]
	[TestCase(true, true, 100, 0, 100, 410)]
	[TestCase(false, true, 0, 0, 0, 0)]
	[TestCase(false, true, 0, 0, 0, 0)]
	[TestCase(false, true, 0, 100, 0, 100)]
	[TestCase(false, true, 100, 0, 100, 0)]
	[TestCase(true, false, 0, 0, 0, 0)]
	[TestCase(true, false, 0, 0, 0, 0)]
	[TestCase(true, false, 0, 0, 0, 0)]
	[TestCase(true, false, 0, 100, 0, 100)]
	[TestCase(true, false, 100, 0, 100, 0)]
	public void ShouldDrawTooltip(
		bool tooltipEnabled,
		bool validItem,
		int x,
		int y,
		int expectedX,
		int expectedY
	)
	{
		ConfigModel.Instance.EnableTooltip = tooltipEnabled;
	
		var sb = new SpriteBatch(null, 0);
		var result = _tooltipService.DrawTooltip(validItem ? _validObject : _invalidObject, sb, x, y);
		
		_mockDrawSupplyBarHelper.Verify(m=> m.DrawSupplyBar(sb, It.IsAny<int>(),  It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), ), Times.Once);
		
		Assert.That(result, Is.EqualTo(new Point(expectedX, expectedY)));
	}
}