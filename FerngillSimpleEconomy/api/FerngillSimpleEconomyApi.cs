using fse.core.services;
using StardewValley;

namespace fse.core.api;

public class FerngillSimpleEconomyApi(IEconomyService? economyService) : IFerngillSimpleEconomyApi
{
	public bool IsLoaded() => economyService?.Loaded ?? false;

	public void AdjustSupply(Object obj, int amount)
	{
		economyService?.AdjustSupply(obj, amount);
	}

	public void AdjustDelta(Object obj, int amount)
	{
		economyService?.AdjustDelta(obj, amount);
	}

	public int? GetSupply(Object obj) => economyService?.GetItemModelFromObject(obj)?.Supply;

	public int? GetDelta(Object obj) => economyService?.GetItemModelFromObject(obj)?.DailyDelta;

	public bool ItemIsInEconomy(Object obj) => economyService?.GetItemModelFromObject(obj) != null;
}

