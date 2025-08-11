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

internal sealed class PendingAdjustSave {
	public Dictionary<string, int> Counts { get; set; } = new();
	public long ApplyOnDaysPlayed { get; set; }
}

public class DayEndHandler(
		IModHelper helper,
		IMonitor monitor,
		IEconomyService economyService
) : IHandler {

	private const string SaveKey = "fse.pending.adjust";
	internal static readonly Dictionary<string, int> PendingAdjust = new();

	public void Register()
	{
		helper.Events.GameLoop.DayEnding += (_, _) =>
			SafeAction.Run(GameLoopOnDayEnding, monitor, nameof(GameLoopOnDayEnding));

		helper.Events.GameLoop.DayStarted += (_, _) =>
			SafeAction.Run(OnDayStarted, monitor, nameof(OnDayStarted));
	}

	private void SavePending()
				=> helper.Data.WriteSaveData(SaveKey, new PendingAdjustSave { Counts = new(PendingAdjust) });

	private void GameLoopOnDayEnding()
	{
		if (!Game1.player.IsMainPlayer)
			return;

		var farmers = Game1.getAllFarmers();
		if (!Game1.player.team.useSeparateWallets.Value)
			farmers = [farmers.First()];

		PendingAdjust.Clear();

		foreach (var farmer in farmers)
		{
			foreach (var item in Game1.getFarm().getShippingBin(farmer))
			{
				if (item is not Object o) continue;

				if (ConfigModel.Instance.ShippingPricingMode == PricingMode.Instant)
				{
					var model = economyService.GetItemModelFromObject(o);
					if (model is null) continue;

					// Add Shipped Items (via Shipping Bin)
					PendingAdjust[model.ObjectId] = PendingAdjust.TryGetValue(model.ObjectId, out var q) ? q + o.Stack : o.Stack;
				}
				else
				{
					// Default Behaviour
					economyService.AdjustSupply(o, o.Stack, notifyPeers: false);
				}
			}
		}

		// Persist so Exit or Crash won’t lose it
		if (PendingAdjust.Count > 0)
		{
			SavePending();
		}

		economyService.HandleDayEnd(new DayModel(Game1.year, SeasonHelper.GetCurrentSeason(), Game1.dayOfMonth));
	}

	private void OnDayStarted()
	{
		var saved = helper.Data.ReadSaveData<PendingAdjustSave>(SaveKey);
		if (saved is not null && saved.Counts.Count > 0 && saved.ApplyOnDaysPlayed == Game1.stats.DaysPlayed)
		{
			foreach (var (objectId, qty) in saved.Counts)
			{
				PendingAdjust[objectId] = qty;
			}
		}

		// Add "Supply" to Shipped Items (via Shipping Bin)
		if (PendingAdjust.Count > 0)
		{
			foreach (var (objectId, qty) in PendingAdjust)
			{
				economyService.AdjustSupply(new Object(objectId, 1), qty, notifyPeers: false);
			}
		}

		economyService.AdvanceOneDay();
	}
}
