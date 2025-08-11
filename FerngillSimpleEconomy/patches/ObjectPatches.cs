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

			// Non-Shop: Apply Normal FSE Pricing
			var basePrice = __result;
			if (ConfigModel.Instance.ShopPricingMode == PricingMode.Instant)
			{
				__result = EconomyService.GetInstantPrice(__instance);
				//__result = SafeAction.Run(() => EconomyService.GetInstantPrice(__instance), basePrice, Monitor);
			}
			else
			{
				__result = SafeAction.Run(() => EconomyService.GetPrice(__instance, basePrice), basePrice, Monitor);
			}
		}

		public override void Register(Harmony harmony) {
			harmony.Patch(
				AccessTools.Method(typeof(Object), nameof(Object.sellToStorePrice)),
				postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(SellToStoreSalePricePostFix))
			);
		}

		public static bool GetSellToStorePriceOfItem_Prefix(Item i, bool countStack, ref int __result) {
			if (!countStack) return true;
			if (ConfigModel.Instance.ShopPricingMode != PricingMode.Instant) return true;
			if (i is not Object obj) return true;

			__result = EconomyService.GetInstantTotal(obj);
			return false;
		}
	}
}
