using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using fse.core.models;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace fse.core.patches
{
	public class ShopMenuPatches : SelfRegisteringPatches
	{
		public static bool AddBuyBackItemPreFix(
			ShopMenu __instance, 
			ISalable sold_item, 
			int sell_unit_price,
			int stack, 
			out BuyBackState __state)
		{
			var buyBackItems = new List<ISalable>();
			
			if (ConfigModel.Instance.PricingMode == PricingMode.Instant)
			{
				// The original method will compare sold items to the buyback list and stack items if the `canStackWith` method returns true.
				// modifying this method via another patch would have unintended consequences. Thankfully the logic plays fairly nicely with
				// removing the item from `buyBackItems` before the original method runs and then adding it back in the postfix.
				// This makes it so that the original method will not stack items with different prices and allows the player to buy back items at the price they were sold at.
				// of note is this does not require removing the item from `itemPriceAndStock` which is convenient to track the price of the item.

				var sameKeys = __instance.buyBackItems
					.Where(key => key.canStackWith(sold_item))
					.Where(__instance.itemPriceAndStock.ContainsKey)
					.Where(key => __instance.itemPriceAndStock[key].Price != sell_unit_price)
					.ToList();
				
				foreach (var key in sameKeys)
				{
					__instance.buyBackItems.Remove(key);
				}

				buyBackItems = sameKeys;
			}
			
			__state = new BuyBackState(buyBackItems, stack); //store the stack size before the original function modifies it

			return true;
		}

		public static void AddBuyBackItemPostFix(
			ShopMenu __instance, 
			ISalable sold_item, 
			int sell_unit_price, 
			BuyBackState __state
		)
		{
			// supply must be adjusted in the postfix as changing the supply in the prefix would result in the price being calculated with the wrong supply
			if (sold_item is Object soldObject)
			{
				EconomyService.AdjustSupply(soldObject, __state.StackSize);
			}

			foreach (var key in __state.BuyBackItems)
			{
				__instance.buyBackItems.Add(key);
			}
		}

		public static void BuyBuybackItemPostFix(ShopMenu __instance, ISalable bought_item, int stack)
		{
			if (bought_item is not Object boughtObject)
			{
				return;
			}

			EconomyService.AdjustSupply(boughtObject, -stack);
		}

		public static void DrawSeedInfo(
			SpriteBatch b,
			// ReSharper disable once InconsistentNaming
			ShopMenu shopMenu
		)
		{
			if (shopMenu.forSale == null)
			{
				return;
			}

			if (!ConfigModel.Instance.EnableShopDisplay)
			{
				return;
			}

			var forSaleButtons = shopMenu.forSaleButtons;
			var forSale = shopMenu.forSale;
			var currItemIdx = shopMenu.currentItemIndex;
			var vanillaBtnWidth = shopMenu.width - 32;
			for (var i = 0; i < forSaleButtons.Count; i++)
			{
				if (currItemIdx + i >= forSale.Count)
				{
					continue;
				}
				var component = forSaleButtons[i];
				var salable = forSale[currItemIdx + i];
				var bounds = component.bounds;
				if (salable is Object { Category: Object.SeedsCategory } obj && EconomyService.GetItemModelFromSeed(obj.ItemId) is ItemModel model)
				{
					if (bounds.Width < vanillaBtnWidth)
					{
						DrawSupplyBarHelper.DrawSupplyBar(b, bounds.X + 96, component.bounds.Y + 20, bounds.Right - bounds.Width / 5, 30, model);
					}
					else
					{
						DrawSupplyBarHelper.DrawSupplyBar(b, bounds.Right - 400, component.bounds.Y + 20, bounds.Right - 200, 30, model);
					}
				}
			}
		}

		public override void Register(Harmony harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.AddBuybackItem)),
				prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(AddBuyBackItemPreFix)),
				postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(AddBuyBackItemPostFix))
			);

			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.BuyBuybackItem)),
				postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(BuyBuybackItemPostFix))
			);

			harmony.Patch(
				AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.draw), [typeof(SpriteBatch)]),
				transpiler: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopDrawingTranspiler))
			);
		}

		public static IEnumerable<CodeInstruction> ShopDrawingTranspiler(IEnumerable<CodeInstruction> steps)
		{
			using var enumerator = steps.GetEnumerator();

			while (enumerator.MoveNext())
			{
				var current = enumerator.Current;

				if (current.opcode == OpCodes.Ldfld && (FieldInfo)current.operand == AccessTools.Field(typeof(ShopMenu), nameof(ShopMenu.downArrow)))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopMenuPatches), nameof(DrawSeedInfo)));
				}

				yield return enumerator.Current;
			}
		}
	}
}