using fse.core.actions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace fse.core.patches;

public class ItemPatches : SelfRegisteringPatches
{
	public static void GetExtraSpaceNeededForTooltipSpecialIcons(Item __instance, int startingHeight, ref Point __result)
	{
		var existingResult = __result;
		__result = SafeAction.Run(() => TooltipService.GetExtraSpaceNeededForTooltipSpecialIcons(__instance, startingHeight, existingResult), __result, Monitor);
	}

	public static void DrawTooltip(Item __instance, SpriteBatch spriteBatch, ref int x, ref int y)
	{
		var existingX = x;
		var existingY = y;

		var offset = SafeAction.Run(() => TooltipService.DrawTooltip(__instance, spriteBatch, existingX, existingY), Point.Zero, Monitor);

		x += offset.X;
		y += offset.Y;
	}

	public override void Register(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Method(typeof(Item), nameof(Item.getExtraSpaceNeededForTooltipSpecialIcons)),
			postfix: new HarmonyMethod(typeof(ItemPatches), nameof(GetExtraSpaceNeededForTooltipSpecialIcons))
		);

		harmony.Patch(
			AccessTools.Method(typeof(Item), nameof(Item.drawTooltip)),
			postfix: new HarmonyMethod(typeof(ItemPatches), nameof(DrawTooltip))
		);
	}
}