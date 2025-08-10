using fse.core.actions;
using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using static fse.core.models.ConfigModel;

namespace fse.core.handlers;

public class DayEndHandler(
	IModHelper helper,
	IMonitor monitor,
	IEconomyService economyService)
	: IHandler
{
	public void Register()
	{
		helper.Events.GameLoop.DayEnding += (_, _) => 
			SafeAction.Run(GameLoopOnDayEnding, monitor, nameof(GameLoopOnDayEnding));
		helper.Events.GameLoop.DayStarted += (_, _) =>
			SafeAction.Run(economyService.AdvanceOneDay, monitor, nameof(economyService.AdvanceOneDay));
	}

	private void GameLoopOnDayEnding()
	{
		if (!Game1.player.IsMainPlayer)
			return;

		var farmers = Game1.getAllFarmers();
		if (!Game1.player.team.useSeparateWallets.Value)
			farmers = [farmers.First()];

		if (ConfigModel.Instance.ShippingPricingMode == PricingMode.Instant)
		{
			var qtyByModel = new Dictionary<ItemModel, int>();
			foreach (var farmer in farmers)
			{
				foreach (var it in Game1.getFarm().getShippingBin(farmer))
				{
					if (it is not Object o) continue;
					var model = economyService.GetItemModelFromObject(o);
					if (model is null) continue;
					qtyByModel[model] = qtyByModel.TryGetValue(model, out var q) ? q + o.Stack : o.Stack;
				}
			}

			foreach (var kv in qtyByModel)
			{
				var model = kv.Key;
				var qty = kv.Value;
				if (qty <= 0) continue;

				var startSupply = model.Supply;
				decimal sum = 0m;
				for (int i = 1; i <= qty; i++)
					sum += model.GetMultiplierAtSupply(startSupply + i);

				var avg = sum / qty;
				model.SetOneNightOverrideMultiplier(avg);
			}
		}

		foreach (var farmer in farmers)
		{
			foreach (var item in Game1.getFarm().getShippingBin(farmer).Where(item => item is Object))
			{
				economyService.AdjustSupply(item as Object, item.Stack, false);
			}
		}

		economyService.HandleDayEnd(new DayModel(Game1.year, SeasonHelper.GetCurrentSeason(), Game1.dayOfMonth));
	}
}