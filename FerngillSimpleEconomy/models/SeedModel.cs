using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FruitTrees;

namespace fse.core.models
{
	public class SeedModel
	{
		public string ObjectId { get; }
		public string CropId { get; }
		public int DaysToGrow { get; init; }
		public Seasons Seasons { get; init; }

		public SeedModel(string id, CropData cropEntry)
		{
			ObjectId = id;

			CropId = cropEntry.HarvestItemId;

			foreach (var stage in cropEntry.DaysInPhase)
			{
				DaysToGrow += stage;
			}

			Seasons = ToSeasons(cropEntry.Seasons);
		}

		public SeedModel(string id, FruitTreeData fruitTree)
		{
			ObjectId = id;

			CropId = fruitTree.Fruit.First().ItemId.Replace("(O)", string.Empty);

			DaysToGrow = 28;

			Seasons = ToSeasons(fruitTree.Seasons);
		}
		
		[Pure]
		private static Seasons ToSeasons(List<Season> seasonList)
		{
			Seasons seasons = 0;
			foreach (var season in seasonList)
			{
				switch (season)
				{
					case Season.Spring: seasons |= Seasons.Spring;
						break;
					case Season.Summer: seasons |= Seasons.Summer;
						break;
					case Season.Fall: seasons |= Seasons.Fall;
						break;
					case Season.Winter: seasons |= Seasons.Winter;
						break;
				}
			}

			return seasons;
		}
	}

[Flags]
	public enum Seasons 
	{
		Spring = 1 << 0,
		Summer = 1 << 1,
		Fall = 1 << 2,
		Winter = 1 << 3,
	}
}