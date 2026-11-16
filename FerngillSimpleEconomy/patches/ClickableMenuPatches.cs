using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace fse.core.patches;

public class ClickableMenuPatches : SelfRegisteringPatches
{
	public static void StoreWidth(int width)
	{
		TooltipService.Width = width;
	}

	//approach: find the first drawTextureBox as that is the background and then pop the stack to copy width, and then re-push
	public static IEnumerable<CodeInstruction> DrawHoverText(IEnumerable<CodeInstruction> steps, ILGenerator generator)
	{
		var matched = false;
		using var enumerator = steps.GetEnumerator();

		while (enumerator.MoveNext())
		{
			var current = enumerator.Current;

			if (!matched && current.Calls(AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawTextureBox), 
					[
						typeof(SpriteBatch),
						typeof(Texture2D),
						typeof(Rectangle),
						typeof(int),
						typeof(int),
						typeof(int),
						typeof(int),
						typeof(Color),
						typeof(float),
						typeof(bool),
						typeof(float),
					]
				)))
			{
				var drawLayerStore = generator.DeclareLocal(typeof(float));
				var drawShadowStore = generator.DeclareLocal(typeof(bool));
				var scaleStore = generator.DeclareLocal(typeof(float));
				var colorStore = generator.DeclareLocal(typeof(Color));
				var heightStore = generator.DeclareLocal(typeof(int));
				
				yield return new CodeInstruction(OpCodes.Stloc, drawLayerStore);
				yield return new CodeInstruction(OpCodes.Stloc, drawShadowStore);
				yield return new CodeInstruction(OpCodes.Stloc, scaleStore);
				yield return new CodeInstruction(OpCodes.Stloc, colorStore);
				yield return new CodeInstruction(OpCodes.Stloc, heightStore);
				
				yield return new CodeInstruction(OpCodes.Dup);
				yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ClickableMenuPatches), nameof(StoreWidth)));
				
				yield return new CodeInstruction(OpCodes.Ldloc, heightStore);
				yield return new CodeInstruction(OpCodes.Ldloc, colorStore);
				yield return new CodeInstruction(OpCodes.Ldloc, scaleStore);
				yield return new CodeInstruction(OpCodes.Ldloc, drawShadowStore);
				yield return new CodeInstruction(OpCodes.Ldloc, drawLayerStore);
				
				matched = true;
			}

			yield return enumerator.Current;
		}
	}

	public override void Register(Harmony harmony)
	{
		harmony.Patch( 
			AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), 
				[
					typeof(SpriteBatch),
					typeof(StringBuilder),
					typeof(SpriteFont),
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(string),
					typeof(int),
					typeof(string[]),
					typeof(Item),
					typeof(int),
					typeof(string),
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(float),
					typeof(CraftingRecipe),
					typeof(IList<Item>),
					typeof(Texture2D),
					typeof(Rectangle?),
					typeof(Color?),
					typeof(Color?),
					typeof(float),
					typeof(int),
					typeof(int),
				]
			),
			transpiler: new HarmonyMethod(typeof(ClickableMenuPatches), nameof(DrawHoverText))
		);
	}
}