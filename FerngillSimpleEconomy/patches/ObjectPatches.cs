using fse.core.actions;
using fse.core.models;
using HarmonyLib;
using Object = StardewValley.Object;
using StardewValley;
using StardewValley.Menus;
using System;
using static fse.core.models.ConfigModel;

namespace fse.core.patches
{
	public class ObjectPatches : SelfRegisteringPatches
	{
		public static void SellToStoreSalePricePostFix(Object __instance, ref int __result)
		{
			// Instant Price
			if (Game1.activeClickableMenu is ShopMenu &&
					ConfigModel.Instance.ShopPricingMode == PricingMode.Instant) {
				var model = EconomyService.GetItemModelFromObject(__instance);
				if (model != null) {
					int basePrice = __result;
					decimal mult = model.GetMultiplierAtSupply(model.Supply + 1);
					__result = (int)Math.Round(basePrice * mult, 0, MidpointRounding.ToEven);
					return;
				}
			}

			int baseP = __result;
			__result = SafeAction.Run(() => EconomyService.GetPrice(__instance, baseP), baseP, Monitor);
		}

		public override void Register(Harmony harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(Object), nameof(Object.sellToStorePrice)),
				postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(SellToStoreSalePricePostFix))
			);
		}
	}
}