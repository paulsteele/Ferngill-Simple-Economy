using fse.core.actions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace fse.core.patches;

public class ObjectPatches : SelfRegisteringPatches
{
	public static void SellToStoreSalePricePostFix(Object __instance, ref int __result)
	{
		var basePrice = __result;
		__result = SafeAction.Run(() => EconomyService.GetPrice(__instance, basePrice), __result, Monitor);
	}

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
			AccessTools.Method(typeof(Object), nameof(Object.sellToStorePrice)),
			postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(SellToStoreSalePricePostFix))
		);

		harmony.Patch(
			AccessTools.Method(typeof(Item), nameof(Item.getExtraSpaceNeededForTooltipSpecialIcons)),
			postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(GetExtraSpaceNeededForTooltipSpecialIcons))
		);

		harmony.Patch(
			AccessTools.Method(typeof(Item), nameof(Item.drawTooltip)),
			postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(DrawTooltip))
		);
	}
}