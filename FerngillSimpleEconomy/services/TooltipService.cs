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
	public Point GetExtraSpaceNeededForTooltipSpecialIcons(Item item, int startingHeight, Point result)
	{
		if (!ConfigModel.Instance.EnableTooltip || item is not Object obj)
		{
			return result;
		}

		var offset = economyService.GetItemModelFromObject(obj) == null ? 0 : 110;

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
		
		drawSupplyBarHelper.DrawSupplyBar(spriteBatch, x+15, y+20, x+240, Game1.tileSize / 2, model);
		
		return new Point(0, 110);
	}
}