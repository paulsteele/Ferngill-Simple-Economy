// DayEndHandler.cs
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
		IEconomyService economyService
) : IHandler {

	private readonly Dictionary<string, int> _pendingAdjust = new();

	public void Register()
	{
		helper.Events.GameLoop.DayEnding += (_, _) =>
				SafeAction.Run(GameLoopOnDayEnding, monitor, nameof(GameLoopOnDayEnding));

		helper.Events.GameLoop.DayStarted += (_, _) =>
				SafeAction.Run(OnDayStarted, monitor, nameof(OnDayStarted));
	}

	private void GameLoopOnDayEnding()
	{
		if (!Game1.player.IsMainPlayer)
			return;

		var farmers = Game1.getAllFarmers();
		if (!Game1.player.team.useSeparateWallets.Value)
			farmers = [farmers.First()];

		foreach (var farmer in farmers) {
			foreach (var item in Game1.getFarm().getShippingBin(farmer))
			{
				if (item is not Object o) continue;

				if (ConfigModel.Instance.ShippingPricingMode == PricingMode.Instant)
				{
					var model = economyService.GetItemModelFromObject(o);
					if (model is null) continue;

					// Add Shipped Items (via Shipping Bin)
					_pendingAdjust[model.ObjectId] =
							_pendingAdjust.TryGetValue(model.ObjectId, out var q) ? q + o.Stack : o.Stack;
				}
				else
				{
					// Default Behaviour
					economyService.AdjustSupply(o, o.Stack, notifyPeers: false);
				}
			}
		}

		economyService.HandleDayEnd(new DayModel(Game1.year, SeasonHelper.GetCurrentSeason(), Game1.dayOfMonth));
	}

	private void OnDayStarted()
	{
		// Add "Supply" to Shipped Items (via Shipping Bin)
		if (_pendingAdjust.Count > 0)
		{
			foreach (var (objectId, qty) in _pendingAdjust)
			{
				economyService.AdjustSupply(new Object(objectId, 1), qty, notifyPeers: false);
			}

			_pendingAdjust.Clear();
		}

		economyService.AdvanceOneDay();
	}
}
