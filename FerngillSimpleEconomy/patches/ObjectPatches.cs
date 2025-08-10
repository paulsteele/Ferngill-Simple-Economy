using fse.core.actions;
using fse.core.models;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System;
using static fse.core.models.ConfigModel;
using Econ = fse.core.services.EconomyService;
using Object = StardewValley.Object;

namespace fse.core.patches {
	public class ObjectPatches : SelfRegisteringPatches {
		public static void SellToStoreSalePricePostFix(Object __instance, ref int __result) {
			if (Econ.BypassPricePatch) return;

			if (Game1.activeClickableMenu is ShopMenu &&
					ConfigModel.Instance.ShopPricingMode == PricingMode.Instant) {
				var model = EconomyService.GetItemModelFromObject(__instance);
				if (model != null) {
					int baseUnit = __result; // Vanilla Base Unit
					decimal mNext = model.GetMultiplierAtSupply(model.Supply + 1); // Next Unit Multiplier
					__result = (int)Math.Round(baseUnit * mNext, 0, MidpointRounding.ToEven);
					return;
				}
			}

			// Non-Shop: Apply Normal FSE Pricing
			int baseP = __result;
			__result = SafeAction.Run(() => EconomyService.GetPrice(__instance, baseP), baseP, Monitor);
		}

		public override void Register(Harmony harmony) {
			harmony.Patch(
				AccessTools.Method(typeof(Object), nameof(Object.sellToStorePrice)),
				postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(SellToStoreSalePricePostFix))
			);
		}
	}
}
