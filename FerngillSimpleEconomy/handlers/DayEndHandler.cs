using System.Collections.Generic;
using System.Linq;
using fse.core.actions;
using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using StardewModdingAPI;
using StardewValley;

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
		helper.Events.GameLoop.Saving += (_, _) =>
			SafeAction.Run(OnSaving, monitor, nameof(OnSaving));
	}
	
	private IList<Object> _cachedShippedItems = [];
	private bool _shouldHandleEndOfDay; // super defensive in case midday save mods trigger this

	// This is the correct time to read the shipping bin as after _newDayAfterFade the shipping bin is cleared.
	// Trying to handle supply in _newDayAfterFade is problematic as each client handles their own shipping bin.
	// Instead, the strategy is to cache the items when they are still available and then update the economy right before onSaving
	// so it is saved and quitting the game doesn't lose changes.
	private void GameLoopOnDayEnding()
	{
		if (!Game1.player.IsMainPlayer)
		{
			return;
		}

		var farmers = Game1.getAllFarmers();

		if (!Game1.player.team.useSeparateWallets.Value)
		{
			farmers = [farmers.First()];
		}

		_cachedShippedItems = farmers.SelectMany(f => Game1.getFarm().getShippingBin(f).OfType<Object>()).ToList();
		_shouldHandleEndOfDay = true;
	}

	private void OnSaving()
	{
		if (!Game1.player.IsMainPlayer || !_shouldHandleEndOfDay)
		{
			return;
		}

		foreach (var item in _cachedShippedItems)
		{
			// don't notify as entire economy will be synchronized at the start of the day
			economyService.AdjustSupply(item, item.Stack, false);
		}

		economyService.HandleDayEnd(new DayModel(Game1.year, SeasonHelper.GetCurrentSeason(), Game1.dayOfMonth));
		
		_cachedShippedItems = [];
		_shouldHandleEndOfDay = false;
	}
}