using fse.core.helpers;
using fse.core.models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace fse.core.services;

public interface ITooltipService
{
	Point GetExtraSpaceNeededForTooltipSpecialIcons(Item item, int startingHeight, Point result);
	Point DrawTooltip(Item item, SpriteBatch spriteBatch, int x, int y);
}

public class TooltipService(IEconomyService economyService, IDrawSupplyBarHelper drawSupplyBarHelper) : ITooltipService
{
	private const int TooltipAreaHeight = 110;
	private const int TooltipAreaWidth = 255;
	private const int BarHeight = Game1.tileSize / 2;
	private const int Padding = 15;

	public Point GetExtraSpaceNeededForTooltipSpecialIcons(Item item, int startingHeight, Point result)
	{
		if (!ConfigModel.Instance.EnableTooltip || item is not Object obj)
		{
			return result;
		}

		var offset = economyService.GetItemModelFromObject(obj) == null ? 0 : TooltipAreaHeight;

		if (offset == 0)
		{
			return result;
		}

		if (result.Y == 0)
		{
			result.Y += startingHeight;
		}

		result.Y += offset;

		return result;
	}

	public Point DrawTooltip(Item item, SpriteBatch spriteBatch, int x, int y)
	{
		if (!ConfigModel.Instance.EnableTooltip || item is not Object obj)
		{
			return Point.Zero;
		}
		
		var model = economyService.GetItemModelFromObject(obj);

		if (model == null)
		{
			return Point.Zero;
		}

		drawSupplyBarHelper.DrawSupplyBar(spriteBatch, x + Padding, y + Padding, x + TooltipAreaWidth - Padding, BarHeight, model);
		
		return new Point(0, TooltipAreaHeight);
	}
}