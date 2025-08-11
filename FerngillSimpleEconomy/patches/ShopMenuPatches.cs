using fse.core.models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static fse.core.models.ConfigModel;
using Econ = fse.core.services.EconomyService;
using Object = StardewValley.Object;

namespace fse.core.patches {
	public class ShopMenuPatches : SelfRegisteringPatches {
		// Correct Payout
		public static void AddBuybackItem_Postfix(ShopMenu __instance, ISalable sold_item, int sell_unit_price) {
			if (ConfigModel.Instance.ShopPricingMode != PricingMode.Instant) return;
			if (sold_item is not Object obj) return;

			int quantity = obj.Stack;

			var model = EconomyService.GetItemModelFromObject(obj);
			if (model is null) return;

			// Vanilla base Unit Price
			int baseUnit;
			try { Econ.BypassPricePatch = true; baseUnit = obj.sellToStorePrice(); }
			finally { Econ.BypassPricePatch = false; }

			int vanillaTotal = sell_unit_price * quantity;
			int wantedTotal = EconomyService.GetInstantTotal(obj);
			int delta = wantedTotal - vanillaTotal;
			if (delta != 0) Game1.player.Money += delta;

			EconomyService.AdjustSupply(obj, quantity);
		}

		public static void BuyBuybackItem_Postfix(ISalable bought_item, int price, int stack) {
			if (bought_item is Object o && stack > 0)
				EconomyService.AdjustSupply(o, -stack);
		}

		public static void DrawSeedInfo(SpriteBatch b, ShopMenu shopMenu) {
			if (shopMenu.forSale == null) return;
			if (!ConfigModel.Instance.EnableShopDisplay) return;

			var forSaleButtons = shopMenu.forSaleButtons;
			var forSale = shopMenu.forSale;
			int currItemIdx = shopMenu.currentItemIndex;
			int vanillaBtnWidth = shopMenu.width - 32;

			for (int i = 0; i < forSaleButtons.Count; i++) {
				if (currItemIdx + i >= forSale.Count) continue;

				ClickableComponent component = forSaleButtons[i];
				ISalable salable = forSale[currItemIdx + i];
				Rectangle bounds = component.bounds;

				if (salable is Object { Category: Object.SeedsCategory } obj && EconomyService.GetItemModelFromSeed(obj.ItemId) is ItemModel model) {
					if (bounds.Width < vanillaBtnWidth)
						DrawSupplyBarHelper.DrawSupplyBar(b, bounds.X + 96, component.bounds.Y + 20, bounds.Right - bounds.Width / 5, 30, model);
					else
						DrawSupplyBarHelper.DrawSupplyBar(b, bounds.Right - 400, component.bounds.Y + 20, bounds.Right - 200, 30, model);
				}
			}
		}

		public override void Register(Harmony harmony) {
			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.AddBuybackItem)),
				postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(AddBuybackItem_Postfix))
			);

			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.BuyBuybackItem)),
				postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(BuyBuybackItem_Postfix))
			);

			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.draw), new[] { typeof(SpriteBatch) }),
				transpiler: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopDrawingTranspiler))
			);
		}

		public static IEnumerable<CodeInstruction> ShopDrawingTranspiler(IEnumerable<CodeInstruction> steps) {
			using var e = steps.GetEnumerator();
			while (e.MoveNext()) {
				var cur = e.Current;

				if (cur.opcode == OpCodes.Ldfld &&
						(FieldInfo)cur.operand == AccessTools.Field(typeof(ShopMenu), nameof(ShopMenu.downArrow))) {
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopMenuPatches), nameof(DrawSeedInfo)));
				}

				yield return cur;
			}
		}
	}
}
