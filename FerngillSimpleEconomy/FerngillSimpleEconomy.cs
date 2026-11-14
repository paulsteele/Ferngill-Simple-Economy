using fse.core.api;
using fse.core.handlers;
using fse.core.helpers;
using fse.core.models;
using fse.core.patches;
using fse.core.services;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace fse.core;

// ReSharper disable once UnusedType.Global
public class FerngillSimpleEconomy : Mod
{
	private IFerngillSimpleEconomyApi? _api;

	public override void Entry(IModHelper helper)
	{
		ConfigModel.Instance = helper.ReadConfig<ConfigModel>();
		var fileService = new FileService();
		var contentPackService = new ContentPackService(Monitor, helper, fileService);
		var equivalentItemsService = new EquivalentItemsService(contentPackService);
		var multiplayerService = new MultiplayerService(helper);
		var seedService = new SeedService(Monitor);
		var fishService = new FishService(Monitor);
		var artisanService = new ArtisanService(Monitor, helper, contentPackService, equivalentItemsService);
		var normalDistributionService = new NormalDistributionService();
		var updateFrequencyService = new UpdateFrequencyService();
		var economyService = new EconomyService(helper, Monitor, multiplayerService, fishService, seedService, artisanService, normalDistributionService, updateFrequencyService, equivalentItemsService, contentPackService);
		var drawSupplyBarHelper = new DrawSupplyBarHelper(economyService);
		var forecastMenuService = new ForecastMenuService(helper, economyService, new DrawTextHelper(), drawSupplyBarHelper);
		var betterGameMenuService = new BetterGameMenuService(ModManifest, helper, forecastMenuService);
		var iconicFrameworkService = new IconicFrameworkService(helper, forecastMenuService);
		var starControlService = new StarControlService(ModManifest, helper, forecastMenuService);
		var genericConfigMenuService = new GenericConfigMenuService(helper, ModManifest, economyService, contentPackService);
		var tooltipService = new TooltipService(economyService, drawSupplyBarHelper);
		RegisterPatches(economyService, drawSupplyBarHelper, tooltipService);
		RegisterHandlers(
			helper, 
			economyService, 
			forecastMenuService, 
			betterGameMenuService, 
			iconicFrameworkService, 
			starControlService, 
			genericConfigMenuService, 
			multiplayerService
		);
		helper.ConsoleCommands.Add("fse_reset", "Fully Resets Ferngill Simple Economy", (_, _) =>
		{
			if (!Game1.player.IsMainPlayer)
			{
				return;
			}

			economyService.Reset(true, true, SeasonHelper.GetCurrentSeason());
			economyService.AdvanceOneDay();
		});
		helper.ConsoleCommands.Add("fse_reset_daily", "Resets Daily Supply Change of Ferngill Simple Economy", (_, _) =>
		{
			if (!Game1.player.IsMainPlayer)
			{
				return;
			}

			economyService.Reset(false, true, SeasonHelper.GetCurrentSeason());
			economyService.AdvanceOneDay();
		});

		_api = new FerngillSimpleEconomyApi(economyService);
	}

	private void RegisterPatches
	(
		IEconomyService economyService, 
		IDrawSupplyBarHelper drawSupplyBarHelper,
		ITooltipService tooltipService
	)
	{
		var harmony = new Harmony(ModManifest.UniqueID);

		SelfRegisteringPatches.Initialize(economyService, Monitor, drawSupplyBarHelper, tooltipService);
		new ObjectPatches().Register(harmony);
		new ShopMenuPatches().Register(harmony);
	}

	private void RegisterHandlers
	(
		IModHelper helper, 
		EconomyService economyService, 
		ForecastMenuService forecastMenuService, 
		BetterGameMenuService betterGameMenuService,
		IconicFrameworkService iconicFrameworkService,
		StarControlService starControlService,
		GenericConfigMenuService genericConfigMenuService,
		MultiplayerService multiplayerService
	)
	{
		new DayEndHandler(helper, Monitor, economyService).Register();
		new SaveLoadedHandler(helper, Monitor, economyService).Register();
		new GameLoadedHandler(helper, Monitor, betterGameMenuService, iconicFrameworkService, starControlService, genericConfigMenuService).Register();
		new GameMenuLoadedHandler(helper, Monitor, forecastMenuService).Register();
		new MultiplayerHandler(helper, economyService, multiplayerService).Register();
		new HotkeyHandler(helper, forecastMenuService).Register();
	}

	public override object? GetApi() => _api;
}
