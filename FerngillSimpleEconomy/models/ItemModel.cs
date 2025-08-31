using System;
using System.Text.Json.Serialization;
using fse.core.helpers;
using Object = StardewValley.Object;

namespace fse.core.models;

[method: JsonConstructor]
public class ItemModel(string objectId)
{
	private int _dailyDelta;
	private int _supply;

	//cache the multiplier at the beginning of the day to keep prices consistent throughout the day
	private decimal _cachedMultiplier = 1m;

	[JsonInclude] public string ObjectId => objectId;

	[JsonInclude]
	public int Supply
	{
		get => _supply;
		set => _supply = BoundsHelper.EnsureBounds(value, ConfigModel.MinSupply, ConfigModel.MaxSupply);
	}

	[JsonInclude]
	public int DailyDelta
	{
		get => _dailyDelta;
		set => _dailyDelta = BoundsHelper.EnsureBounds(value, ConfigModel.Instance.MinDelta, ConfigModel.Instance.MaxDelta);
	}

	public void AdvanceOneDay()
	{
		Supply += DailyDelta;
		UpdateMultiplier();
	}

	public void UpdateMultiplier()
	{
		_cachedMultiplier = GetMultiplier(Supply);
	}

	private static decimal GetMultiplier(int supply)
	{
		var ratio = 1 - (Math.Min(supply, ConfigModel.Instance.MaxCalculatedSupply) / (decimal)ConfigModel.Instance.MaxCalculatedSupply);
		var percentageRange = ConfigModel.Instance.MaxPercentage - ConfigModel.Instance.MinPercentage;

		return (ratio * percentageRange) + ConfigModel.Instance.MinPercentage;
	}

	private static decimal GetMultiplier(int supply, int quantity)
	{
		if (supply > ConfigModel.Instance.MaxCalculatedSupply)
		{
			return ConfigModel.Instance.MinPercentage;
		}

		if (quantity <= 1)
		{
			return GetMultiplier(supply);
		}

		var endingSupply = supply + quantity - 1;
		
		var numberOfCappedItems = Math.Max(0, endingSupply - ConfigModel.Instance.MaxCalculatedSupply);
		var numberOfVariableItems = quantity - numberOfCappedItems;

		var variableMultiplier = (GetMultiplier(supply) + GetMultiplier(endingSupply)) / 2; // this is valid because GetMultiplier is linear for non capped items
		var cappedMultiplier = ConfigModel.Instance.MinPercentage;

		return ((variableMultiplier * numberOfVariableItems) + (cappedMultiplier * numberOfCappedItems)) / quantity;
	}

	public decimal GetPrice(int basePrice, int quantity)
	{
		var multiplier = ConfigModel.Instance.PricingMode switch
		{
			PricingMode.Batch => _cachedMultiplier,
			_ => GetMultiplier(Supply, quantity),
		};

		return basePrice * multiplier;
	}

	public void CapSupply()
	{
		Supply = Math.Min(Supply, ConfigModel.Instance.MaxCalculatedSupply);
	}
		
	private Object? _objectInstance;
	public Object GetObjectInstance(int stack)
	{
		if (_objectInstance != null && _objectInstance.Stack == stack)
		{
			return _objectInstance;
		}
		_objectInstance = new Object(ObjectId, stack);
		return _objectInstance;
	}
}