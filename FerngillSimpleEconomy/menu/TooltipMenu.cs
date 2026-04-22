using System;
using System.Linq;
using fse.core.extensions;
using fse.core.helpers;
using fse.core.models;
using fse.core.services;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace fse.core.menu;

public interface ITooltipMenu
{
	void PostRenderHud(RenderedHudEventArgs _);
	void PostRenderGui(RenderedActiveMenuEventArgs _);
}

public class TooltipMenu(
	IModHelper helper,
	IEconomyService econService,
	IDrawSupplyBarHelper drawSupplyBarHelper,
	IBetterGameMenuService betterGameMenuService
	) : ITooltipMenu
{
	private readonly bool _isUiInfoSuiteLoaded = helper.ModRegistry.IsLoaded("Annosz.UiInfoSuite2");

	public void PostRenderHud(RenderedHudEventArgs e) =>
		SafeRenderTooltip(true, () => Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault()?.hoverItem);

	public void PostRenderGui(RenderedActiveMenuEventArgs e) =>
		SafeRenderTooltip(false, () => GetHoveredItemFromMenu(Game1.activeClickableMenu));

	private void SafeRenderTooltip(bool activeClickableShouldBeNull, Func<Item?> retriever)
	{
		if (!ConfigModel.Instance.EnableTooltip)
		{
			return;
		}
		if ((Game1.activeClickableMenu == null) != activeClickableShouldBeNull)
		{
			return;
		}

		var item = retriever();
		if (item != null)
		{
			PopulateHoverTextBoxAndDraw(item);
		}
	}

	private Item? GetHoveredItemFromMenu(IClickableMenu menu)
	{
		var page = betterGameMenuService.GetCurrentPage(menu);
		if (page is InventoryPage inventoryPage)
		{
			return inventoryPage.hoveredItem;
		}

		if (menu is MenuWithInventory inventoryMenu)
		{
			return inventoryMenu.hoveredItem;
		}

		return null;
	}

	private void PopulateHoverTextBoxAndDraw(Item? item)
	{
		if (item is not Object obj)
		{
			return;
		}
		var model = econService.GetItemModelFromObject(obj);

		if (model == null)
		{
			return;
		}

		DrawHoverTextBox(model);
	}

	private void DrawHoverTextBox(ItemModel model)
	{
		const int toolbarXPadding = 20;
		const int toolbarYPadding = 15;
		var tooltipRect = new Rectangle(helper.Input.GetCursorPosition().GetUiScaledPosition().ToPoint(), new Point(260, 110));

		//So that the tooltips don't overlap
		if (_isUiInfoSuiteLoaded)
		{
			tooltipRect.X -= 140;
		}

		Utility.makeSafe(ref tooltipRect);

		IClickableMenu.drawTextureBox(
			Game1.spriteBatch, 
			Game1.menuTexture, 
			new Rectangle(0, 256, 60, 60), 
			tooltipRect.X, 
			tooltipRect.Y, 
			tooltipRect.Width, 
			tooltipRect.Height, 
			Color.White
		);
		drawSupplyBarHelper.DrawSupplyBar(
			Game1.spriteBatch, 
			tooltipRect.X + toolbarXPadding, 
			tooltipRect.Y + toolbarYPadding, 
			tooltipRect.X + tooltipRect.Width - toolbarXPadding, 
			Game1.tileSize / 2, 
			model
		);
	}
}