using System.Collections.Generic;
using StardewValley;

namespace fse.core.models;

public record BuyBackState(List<ISalable> BuyBackItems, int StackSize);