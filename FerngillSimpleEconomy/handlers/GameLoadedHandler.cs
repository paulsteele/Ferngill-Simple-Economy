using fse.core.actions;
using fse.core.services;
using GenericModConfigMenu;
using Leclair.Stardew.BetterGameMenu;
using LeFauxMods.Common.Integrations.IconicFramework;
using StarControl;
using StardewModdingAPI;

namespace fse.core.handlers;

public class GameLoadedHandler(
	IModHelper helper,
	IMonitor monitor,
	IBetterGameMenuService betterGameMenuService,
	IIconicFrameworkService iconicFrameworkService,
	IStarControlService starControlService,
	IGenericConfigMenuService genericConfigMenuService
) : IHandler
{
	public void Register()
	{
		helper.Events.GameLoop.GameLaunched += (_, _) => SafeAction.Run(OnLaunched, monitor, nameof(OnLaunched));
	}

	private void OnLaunched()
	{
		RegisterBetterGameMenu();
		RegisterIconicAndStarControl();
		RegisterGenericConfigMenu();
	}
	
	private void RegisterIconicAndStarControl()
	{
		var iconicFramework = helper.ModRegistry.GetApi<IIconicFrameworkApi>("furyx639.ToolbarIcons");
		var starControlApi = helper.ModRegistry.GetApi<IStarControlApi>("focustense.StarControl");
		
		iconicFrameworkService.Register(iconicFramework);
		starControlService.Register(starControlApi, iconicFramework);
	}

	private void RegisterBetterGameMenu()
	{
		var betterGameMenuApi = helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
		betterGameMenuService.Register(betterGameMenuApi);
	}

	private void RegisterGenericConfigMenu()
	{
		var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
		genericConfigMenuService.Register(configMenu);
	}
}
