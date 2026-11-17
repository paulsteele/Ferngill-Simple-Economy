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
	int Width { set; }
}

public class TooltipService(IEconomyService economyService, IDrawSupplyBarHelper drawSupplyBarHelper) : ITooltipService
{
	private const int BarHeight = Game1.tileSize / 2;
	private const int VerticalPadding = 20;
	private const int TooltipAreaHeight = BarHeight + VerticalPadding + 34;
	private const int HorizontalPadding = 18;
	public int Width { get; set; }

	public Point GetExtraSpaceNeededForTooltipSpecialIcons(Item item, int startingHeight, Point result)
	{
		if (!ConfigModel.Instance.EnableTooltip || item is not Object obj)
		{
			return result;
		}

		var offset = GetRelevantItemModelForTooltip(obj) == null ? 0 : TooltipAreaHeight;

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
		
		var model = GetRelevantItemModelForTooltip(obj);

		if (model == null)
		{
			return Point.Zero;
		}

		drawSupplyBarHelper.DrawSupplyBar(spriteBatch, x + HorizontalPadding, y + VerticalPadding, x + Width - HorizontalPadding - 12, BarHeight, model);
		
		return new Point(0, TooltipAreaHeight);
	}

	private ItemModel? GetRelevantItemModelForTooltip(Object obj)
	{
		if (economyService.GetItemModelFromObject(obj) is { } model)
		{
			return model;
		}

		if (economyService.GetItemModelFromSeed(obj.ItemId) is { } seedModel)
		{
			return seedModel;
		}

		return null;
	}
}