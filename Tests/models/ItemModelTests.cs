using fse.core.models;

namespace Tests.models;

public class ItemModelTests : HarmonyTestBase
{
	private ItemModel _itemModel;

	[SetUp]
	public override void Setup()
	{
		base.Setup();
		ConfigModel.Instance = new ConfigModel
		{
			MaxCalculatedSupply = 200,
			MinDelta = -100,
			MaxDelta = 20,
			MinPercentage = 0.2m,
			MaxPercentage = 0.8m,
		};

		_itemModel = new ItemModel("item1")
		{
			Supply = 100,
			DailyDelta = 10,
		};
	}

	[Test]
	public void TestAdvanceOneDay()
	{
		_itemModel.AdvanceOneDay();
		Assert.That(_itemModel.Supply, Is.EqualTo(110));

		_itemModel.DailyDelta = -20;
		
		_itemModel.AdvanceOneDay();
		Assert.That(_itemModel.Supply, Is.EqualTo(90));
	}

	[Test]
	public void TestUpdateMultiplier()
	{
		_itemModel.UpdateMultiplier();
		Assert.That(_itemModel.GetPrice(100, 1), Is.EqualTo(50));
		_itemModel.Supply = 50;
		Assert.That(_itemModel.GetPrice(100, 20), Is.EqualTo(50));
		_itemModel.UpdateMultiplier();
		Assert.That(_itemModel.GetPrice(100, 100), Is.EqualTo(65));
	}

	[Test]
	public void TestCapSupply()
	{
		_itemModel.Supply = 250;
		_itemModel.CapSupply();
		Assert.That(_itemModel.Supply, Is.EqualTo(200));
	}

	[Test]
	public void TestGetObjectInstance([Values(1, 5, 10)] int stack)
	{
		var obj = _itemModel.GetObjectInstance(stack);
		Assert.That(obj.ItemId, Is.EqualTo(_itemModel.ObjectId));
		Assert.That(obj.Stack, Is.EqualTo(stack));
	}

	private static IEnumerable<TestCaseData> TestGetPriceSource()
	{
		yield return new TestCaseData(PricingMode.Batch, .3m, 1.3m, 1000, 750, 1, 100).Returns(55m);
		yield return new TestCaseData(PricingMode.Batch, .3m, 1.3m, 1000, 250, 1, 100).Returns(105m);
		yield return new TestCaseData(PricingMode.Batch, .2m, 5m, 1000, 500, 1, 100).Returns(260m);
		yield return new TestCaseData(PricingMode.Batch, 6.5m, 7m, 1000, 1000, 1, 100).Returns(650m);
		yield return new TestCaseData(PricingMode.Batch, 3.5m, 5m, 1000, 0, 1, 100).Returns(500m);
		
		yield return new TestCaseData(PricingMode.Instant, .3m, 1.3m, 1000, 750, 1, 100).Returns(55m);
		yield return new TestCaseData(PricingMode.Instant, .3m, 1.3m, 1000, 250, 1, 100).Returns(105m);
		yield return new TestCaseData(PricingMode.Instant, .2m, 5m, 1000, 500, 1, 100).Returns(260m);
		yield return new TestCaseData(PricingMode.Instant, 6.5m, 7m, 1000, 1000, 1, 100).Returns(650m);
		yield return new TestCaseData(PricingMode.Instant, 3.5m, 5m, 1000, 0, 1, 100).Returns(500m);
		
		yield return new TestCaseData(PricingMode.Batch, .3m, 1.3m, 1000, 750, 250, 100).Returns(55m);
		yield return new TestCaseData(PricingMode.Batch, .3m, 1.3m, 1000, 250, 250, 100).Returns(105m);
		yield return new TestCaseData(PricingMode.Batch, .2m, 5m, 1000, 500, 250, 100).Returns(260m);
		yield return new TestCaseData(PricingMode.Batch, 6.5m, 7m, 1000, 1000, 250, 100).Returns(650m);
		yield return new TestCaseData(PricingMode.Batch, 3.5m, 5m, 1000, 0, 250, 100).Returns(500m);
		
		yield return new TestCaseData(PricingMode.Instant, .3m, 1.3m, 1000, 750, 250, 100).Returns(42.55m);
		yield return new TestCaseData(PricingMode.Instant, .3m, 1.3m, 1000, 250, 250, 100).Returns(92.55m);
		yield return new TestCaseData(PricingMode.Instant, .2m, 5m, 1000, 500, 250, 100).Returns(200.24m);
		yield return new TestCaseData(PricingMode.Instant, 6.5m, 7m, 1000, 1000, 250, 100).Returns(650m);
		yield return new TestCaseData(PricingMode.Instant, 3.5m, 5m, 1000, 0, 250, 100).Returns(481.325m);
	}
	
	[TestCaseSource(nameof(TestGetPriceSource))]
	public decimal TestGetPrice(
		PricingMode pricingMode,
		decimal minMultiplier,
		decimal maxMultiplier,
		int maxCalculatedSupply,
		int supply,
		int stackSize,
		int basePrice
	)
	{
		ConfigModel.Instance.PricingMode = pricingMode;
		ConfigModel.Instance.MinPercentage = minMultiplier;
		ConfigModel.Instance.MaxPercentage = maxMultiplier;
		ConfigModel.Instance.MaxCalculatedSupply = maxCalculatedSupply;
		_itemModel.Supply = supply;
		
		_itemModel.UpdateMultiplier();
		return _itemModel.GetPrice(basePrice, stackSize);
	}
	
	private static IEnumerable<TestCaseData> IterativeVsBatchEquivalenceTestCases()
	{
		yield return new TestCaseData(100, 5, 50, 0.2m, 0.8m, 200, 322m);
		yield return new TestCaseData(100, 10, 50, 0.2m, 0.8m, 200, 636.5m);
		yield return new TestCaseData(150, 20, 100, 0.3m, 0.9m, 250, 1911.6m);
		yield return new TestCaseData(200, 1, 0, 0.1m, 0.5m, 100, 100m);
		yield return new TestCaseData(75, 15, 180, 0.4m, 1.2m, 200, 508.5m);
		yield return new TestCaseData(300, 25, 25, 0.2m, 0.8m, 50, 2670m);
		yield return new TestCaseData(500, 500, 900, 0.2m, 0.8m, 1000, 51515m);
		yield return new TestCaseData(500, 500, 1000, 0.2m, 0.8m, 1000, 50000m);
		yield return new TestCaseData(500, 250, 1000, 0.2m, 0.8m, 1000, 25000m);
	}
	
	[TestCaseSource(nameof(IterativeVsBatchEquivalenceTestCases))]
	public void ShouldCalculateEquivalentPricesForIterativeVsBatchInstantMode(
		int basePrice,
		int quantity,
		int initialSupply,
		decimal minPercentage,
		decimal maxPercentage,
		int maxCalculatedSupply,
		decimal expectedTotal
	)
	{
		ConfigModel.Instance.PricingMode = PricingMode.Instant;
		ConfigModel.Instance.MinPercentage = minPercentage;
		ConfigModel.Instance.MaxPercentage = maxPercentage;
		ConfigModel.Instance.MaxCalculatedSupply = maxCalculatedSupply;
		
		// Calculate price using batch method (single call with full quantity)
		_itemModel.Supply = initialSupply;
		_itemModel.UpdateMultiplier();
		var batchPrice = _itemModel.GetPrice(basePrice, quantity);
		
		// Calculate price using iterative method (loop through individual items)
		_itemModel.Supply = initialSupply;
		var iterativeTotal = 0m;
		
		for (var i = 0; i < quantity; i++)
		{
			_itemModel.Supply = initialSupply + i;
			_itemModel.UpdateMultiplier();
			iterativeTotal += _itemModel.GetPrice(basePrice, 1);
		}

		Assert.That(iterativeTotal, Is.EqualTo(batchPrice * quantity));
		Assert.That(iterativeTotal, Is.EqualTo(expectedTotal));
	}
	
	[Test]
	public void ShouldHandleZeroQuantityGracefully()
	{
		ConfigModel.Instance.PricingMode = PricingMode.Instant;
		_itemModel.Supply = 100;
		
		_itemModel.UpdateMultiplier();
		var price = _itemModel.GetPrice(100, 0);
		Assert.That(price, Is.EqualTo(50));
	}
}