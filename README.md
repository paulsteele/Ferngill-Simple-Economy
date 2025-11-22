# Ferngill Simple Economy
Downloads: [Nexus](https://www.nexusmods.com/stardewvalley/mods/21414)

# What is it?

Ferngill Simple Economy at its core is a mod that makes Stardew Valley follow the rules of supply and demand. The goal is that there is no longer an optimal crop to be grown to maximize profits by providing some extra financial challenge to the game.

Other economy mods that I've encountered seem to focus on penalizing the player through taxes. Requiring the player to hold money in reserve to be able to pay at a later date. While this mod does penalize a player if only a small set of crops are ever sold, it also rewards players who diversify and capitalize on crops that are in demand. Potentially making those crops sell for a higher value than vanilla.

Ferngill Simple Economy is also designed such that it has implicit support for any mod that adds crops / items. (See the Compatibility section down below)

Above all, the intent of the mod is to make things more fun, not necessarily follow a perfect economic model. You can play this mod by studying the forecast menu for hours each season to maximize profits or you can simply show up to Pierre's and buy seeds that have a favorable supply to still make good returns.

# How it works (Basic)

## Supply

This is the amount of an item that is in the economy. If an item has a high supply it will sell for less than it normally would. However if an item has a low supply value it can potentially sell for more than it normally would.

Supply will change when

* When you sell an item at a shop
* When you sell an item through the shipping bin
* When the day ends and daily change is applied (see below)

Supply will not change when:

* When an item is bought at a shop (unless it is being bought back)

By default, supply caps at 1000 for price calculations but will still be tracked if you sell more. This means if you sell a lot of an item daily change is unlikely to make the supply go below the "cap". Supply is reset for every item at the start of every year.

In the below image green beans have the lowest supply value so their sale price is relatively better than other vegetables. This doesn't necessarily mean that it is the most profitable. Supply applies a multiplier to the normal sale price of an item. This multiplier is applied in conjunction with things that normally affect price like quality, perks, difficulty settings, and even other mods.
![Green beans low supply](https://staticdelivery.nexusmods.com/mods/1303/images/21414/21414-1711839863-608971690.png)


## Daily Change
This is the amount that supply will change every day without the player selling any of the item. It can be both positive and negative. Daily change, just like supply, is different for each item. But it will reset every season unlike supply. This means you can only plan so far ahead in the year.

In the below image green beans have a negative daily change. This means the supply will lower each day during the season, and in turn prices will be higher the longer you wait to sell in the season.
Price

![Green beans negative daily change](https://staticdelivery.nexusmods.com/mods/1303/images/21414/21414-1711840563-1946592978.png)

## Price
Supply is most of the story when it comes to determining price but there are a couple of points worth mentioning about the price multiplier itself.
1) By default, it is possible to have supply low enough items to sell for higher than they normally would. In my testing, I found that I was getting more money than I normally would in a play-through without this mod.
2) By default, price is locked in day to day, meaning it is possible to sell 20,000 parsnips and have all of them fetch the price as if the supply was 0 if that's where it started in the morning. While this may seem to defeat the point of the mod, I found it made an interesting choice that needed to be made while still supporting large volumes of crops being produced. Do you hoard your crops to maximize profits at the detriment of early season cash flow? Or do you sell as soon as possible and hurt yourself if you sell more of the item later in the season.
3) All items will have a sell price listed in the menu. This shows how much a normal quality item would sell on the current date. For crops that have growth time, that sale price is divided by the number of days it takes to grow and is displayed under the Sell Price Per Day column. Items without known growth cycles will instead have "---" listed. This value does not take into account crops that produce multiple items per harvest. Nor does it take into account regrowth cycles.  Both of these columns do not affect how much an item sells for, they simply provide an easy way to see what crops might be worth growing. To note: cost of seeds is not accounted for in these calculations. There's still a bit of manual work to be done to maximize profits!
![Sell Price Per Day](https://staticdelivery.nexusmods.com/mods/1303/images/21414/21414-1711894282-914518526.png)
 
Looking through the entire menu for crops can be a little daunting when purchasing seeds, so the shop will also display the supply for crops that seeds grow. This supply is not for the seed itself.

![Shop menu with forecast](https://staticdelivery.nexusmods.com/mods/1303/images/21414/21414-1711833120-1924923088.png)

# Compatibility

| Feature                | Status | Notes                                                                                                 |
|------------------------|--------|-------------------------------------------------------------------------------------------------------|
| Installing Mid Save    | ✔️     | Your current crops might not be profitable though! I'd recommend installing on season boundaries      |
| Un-Installing Mid Save | ✔️     | Prices are altered dynamically so there is no persistent information changed in the vanilla save file |
| Multiplayer Support    | ✔️     | As of 1.3.0 multiplayer is supported. (split screen not tested).                                      |
| Controller Support     | ✔️     | As of 1.7.0 controller is supported!. Please report any issues you have!                              |

As mentioned, one of the key aspects of this mod was that modded crops would seamlessly integrate with the economy. Theoretically every modded item that fits into vanilla item categories should be automatically supported. Here you can see Cornucopia crops automatically being imported.

![Cornucopia Integration](https://staticdelivery.nexusmods.com/mods/1303/images/21414/21414-1711833120-1819704406.png)

Specific mods tested:

| Mod                              | Status | Notes                                                                                                                                                                                                                   |
|----------------------------------|--------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Better Balance                   | ✔️     | N/A                                                                                                                                                                                                                     |
| Stardew Valley Balance Overhaul  | ✔️     | N/A                                                                                                                                                                                                                     |
| Cornucopia - More Crops          | ✔️     | N/A                                                                                                                                                                                                                     |
| Informant - Updated and Improved | ✔️     | N/A                                                                                                                                                                                                                     |
| Overgrown Flowery Interface      | ✔️     | Ferngill Simple Economy for Flowery Interface                                                                                                                                                                           |
| UIInfoSuite2                     | ⚠️     | UIInfoSuite2 also adds a tab to the main menu and they overlap. Setting MenuTabOffset in simple economy to 70 should make both tabs accessible. The mod will attempt to do so automatically if it detects UiInfoSuite2  |
| Other mods?                      | ❓      | Please report any compatibility issues you find!                                                                                                                                                                        |

# Configuration
I attempted to make most things in the mod configurable. 

Categories of items that are considered in the economy can be customized starting with version 1.2.0

There are also settings to disable any menu integration since it can likely conflict with other mods.

# Technical Details 
First a note: knowing how the mod works on a technical level is not necessary to enjoy it. I have tweaked the default settings over six separate multi-year playthroughs, so they are fairly reasonable by default. 

## "Randomness"

Ferngill Simple Economy centers around [the bell curve](https://en.wikipedia.org/wiki/Normal_distribution) and has two major distributions that affect how prices are calculated.

The default distributions of supply and daily change can view viewed on Wolfram Alpha : 
* Supply 
* Daily Change

Each distribution has both a mean and standard deviation that can be customized in the config file. In general playing with a crop mod or two, there will almost always be a profitable crop to grow.

## Categories
Items are included in the economy based on the category they are under. By default, the included categories are:

* Forage
* Vegetables
* Minerals
* Fruit
* Fish
* Animal Products
* Artisan Goods
* Basic
* Flowers
Starting from 1.2.0, categories can be customized in the config menu(recommended) / file.

Some special items (e.g., The Legend) are excluded from the economy model.

## Miscellaneous Details

With how the game is coded, generic artisan goods like wines and pickles are technically one item id. To reduce confusion / complexity they are not shown in the menu and are instead calculated given the base item's supply. This also avoids weird situations where supply differences between the base item and artisan item can make processing the item not worth it.

For example, if supply is 0 for strawberries, strawberry wine will have the max multiplier (on top of the normal wine multiplier). While if Blueberries have a maxed supply, Blueberry Wine will have the minimum multiplier.

Special artisan goods (e.g., Caviar) will have their own supply. Rule of thumb: if it's in the menu as its own item, it has its own supply.

# Bug Reporting
Please report all bugs to https://github.com/paulsteele/Ferngill-Supply-And-Demand/issues

# Localization
Pull requests are welcome at https://github.com/paulsteele/Ferngill-Supply-And-Demand/pulls

# Source / Permissions
The source is available at https://github.com/paulsteele/Ferngill-Supply-And-Demand under the MIT License. I'd love if you'd submit improvements through a PR on this mod, but you're free to fork and re-upload however you see fit.

# Credits
## Libraries
* https://numerics.mathdotnet.com/ for the backing algorithms used to seed data.

## Contributors
* Tooltips - https://github.com/lilyzeiset
* BetterGame Menu Support - https://github.com/KhloeLeclair
* Compact Searchable Shop Menu Support - https://github.com/Mushymato
* Configurable Supply / Delta - https://github.com/cidrei & https://github.com/SingularityPotato
* Instant Pricing - https://github.com/BeesQ & https://github.com/SingularityPotato

## Translations
* Korean - cheesecats
* French - https://github.com/CaranudLapin
