﻿using System;
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

	public decimal GetPrice(int basePrice, int stackSize)
	{
		var multiplier = ConfigModel.Instance.PricingMode switch
		{
			PricingMode.Batch => _cachedMultiplier,
			_ => (GetMultiplier(Supply) + GetMultiplier(Math.Max(Supply + (stackSize - 1) , 0))) / 2,
		};

		return basePrice * multiplier;
	}

	public void CapSupply()
	{
		Supply = Math.Min(Supply, ConfigModel.Instance.MaxCalculatedSupply);
	}
		
	private Object? _objectInstance;
	public Object GetObjectInstance() => _objectInstance ??= new Object(ObjectId, 1);
}