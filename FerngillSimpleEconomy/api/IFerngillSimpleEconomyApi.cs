using StardewValley;
// ReSharper disable UnusedMember.Global

namespace fse.core.api;

/// <summary>
/// Public API for Ferngill Simple Economy mod.
/// </summary>
public interface IFerngillSimpleEconomyApi
{
	/// <summary>
	/// Gets whether the economy has been loaded. No actions can be taken if the economy is not loaded.
	/// </summary>
	/// <returns>True if the economy has been loaded, false otherwise.</returns>
	bool IsLoaded();

	/// <summary>
	/// Adjusts the supply value for the specified object.
	/// </summary>
	/// <param name="obj">The object to adjust supply for.</param>
	/// <param name="amount">The amount to adjust supply by (can be negative).</param>
	void AdjustSupply(Object obj, int amount);

	/// <summary>
	/// Adjusts the daily delta value for the specified object.
	/// </summary>
	/// <param name="obj">The object to adjust daily delta for.</param>
	/// <param name="amount">The amount to adjust daily delta by (can be negative).</param>
	void AdjustDelta(Object obj, int amount);

	/// <summary>
	/// Gets the current supply value for the specified object.
	/// </summary>
	/// <param name="obj">The object to get supply for.</param>
	/// <returns>The supply value, or null if the object is not in the economy.</returns>
	int? GetSupply(Object obj);

	/// <summary>
	/// Gets the current daily delta value for the specified object.
	/// </summary>
	/// <param name="obj">The object to get daily delta for.</param>
	/// <returns>The daily delta value, or null if the object is not in the economy.</returns>
	int? GetDelta(Object obj);

	/// <summary>
	/// Checks if the specified object is tracked in the economy.
	/// </summary>
	/// <param name="obj">The object to check.</param>
	/// <returns>True if the object is in the economy, false otherwise.</returns>
	bool ItemIsInEconomy(Object obj);
}
