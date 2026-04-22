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
	void PostRendering(RenderingEventArgs _);
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
	private Item? _hoverItem;
	
	public void PostRendering(RenderingEventArgs e) => _hoverItem = GetHoveredItem();

	public void PostRenderHud(RenderedHudEventArgs e) => SafeRenderTooltip(true);

	public void PostRenderGui(RenderedActiveMenuEventArgs e) => SafeRenderTooltip(false);

	private void SafeRenderTooltip(bool activeClickableShouldBeNull)
	{
		if (!ConfigModel.Instance.EnableTooltip)
		{
			return;
		}
		if ((Game1.activeClickableMenu == null) != activeClickableShouldBeNull)
		{
			return;
		}

		if (_hoverItem != null)
		{
			PopulateHoverTextBoxAndDraw(_hoverItem);
		}
	}

	//hoveredItem on tooltips will only be available during the Rendering event.
	private Item? GetHoveredItem()
	{
		var menu = Game1.activeClickableMenu ?? Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();

		var page = betterGameMenuService.GetCurrentPage(menu);
		if (page is InventoryPage inventoryPage)
		{
			return inventoryPage.hoveredItem;
		}

		return menu switch
		{
			MenuWithInventory inventoryMenu => inventoryMenu.hoveredItem,
			Toolbar toolbar => toolbar.hoverItem,
			_ => null,
		};
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