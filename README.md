# Multi User Chest


## About
Allows multiple players to interact with the same chest at the same time.


## Installation
This mod requires [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) and [Jötunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/).
Extract all files to `BepInEx/plugins/MultiUserChest`

The mod must be installed on the server and all client.
It is ensured that all players have the mod installed with the same mod version, otherwise no connection is possible.

Please report all errors and desyncs if they occur.


## FAQ
- Who needs this installed?
  - The server and every player joining it. You will be disconnected if there is a version mismatch or the mod is not installed somewhere.
- When two players move an item at the exact same time, will it be duplicated?
  - No, only one movement will be accepted.
- Can this be added to an existing game?
  - Yes, you can add and remove this mod anytime.
- Does it work with other modded chests
  - Yes, most likely. But it can make problems when other mods change the behaviour of chests.
- Does it work with mods that change inventory/chest sizes
  - Yes.
- Does it work with mods that add quick stacking / chest sorting
  - Depends on the mod, see [Incompatible Mods](#confirmed-incompatible-mods) and [Compatible Mods](#compatible-mods). If it not listed there, assume it doesn't work.
  - Recommended is [Quick Stack Store Sort Trash Restock](https://valheim.thunderstore.io/package/Goldenrevolver/Quick_Stack_Store_Sort_Trash_Restock) by Goldenrevolver, as it unifies many quick stack features and provides explicit compatibility with Multi User Chest.


## Confirmed Incompatible Mods
These mods cause desync with Multi User Chest and will not startup together
- [QuickStore](https://www.nexusmods.com/valheim/mods/1595)
- [SimpleSort](https://www.nexusmods.com/valheim/mods/584)
- [OdinsQOL](https://valheim.thunderstore.io/package/OdinPlus/OdinsQOL) (only some features like AutoStore are effected, both mods will start up so make sure incomatible features are disabled)


## Compatible Mods
Here are some quick stack mods listed that are compatible with Multi User Chest.
The version number indicates the tested version of the other mod, a different one may also work but isn't confirmed.
- [Quick Stack Store Sort Trash Restock](https://valheim.thunderstore.io/package/Goldenrevolver/Quick_Stack_Store_Sort_Trash_Restock) 1.3.1 (recommended quick stack mod, a higher version very likely works too)
- [SmartContainers](https://www.nexusmods.com/valheim/mods/332) 1.6.4
- [QuickDeposit](https://valheim.thunderstore.io/package/MaGic/Quick_Deposit) 1.0.1
- [QuickStack](https://valheim.thunderstore.io/package/damnsneaker/QuickStack) 0.6.6
- [DepositAnywhere](https://valheim.thunderstore.io/package/Lookenpeepers/DepositAnywhere) 1.2.0
- [Valheim Simple Auto Sort](https://www.nexusmods.com/valheim/mods/1824) 1.0.7


## Links
- [Thunderstore](https://valheim.thunderstore.io/package/MSchmoecker/MultiUserChest/)
- [Nexus](https://www.nexusmods.com/valheim/mods/1766)
- [Github](https://github.com/MSchmoecker/No-Chest-Block)
- Discord: Margmas#9562. Feel free to DM or ping me, for example in the [Jötunn discord](https://discord.gg/DdUt6g7gyA)


## Development
See [contributing](https://github.com/MSchmoecker/No-Chest-Block/blob/master/CONTRIBUTING.md).


## Changelog
0.4.4
- Fixed a compatibility issue with OdinShipPlus. Some of the ships have multiple chests on board, these ships are excluded from MUC
- Fixed a compatibility issue with AdventureBackpacks where the backpack items could be duplicated

0.4.3
- Fixed distant containers could cause errors when other mods tried to access them. These containers have no player assigned (owner) responsible for managing their inventory and will be ignored by MUC

0.4.2
- Fixed an item duplication bug when a player moves a weapon/tool/armor item repeatedly in a shared chest

0.4.1
- Fixed an error when a player tries to place a chest

0.4.0
- Big internal rework for compatibility with other mods: instead of patching GUI methods, mainly inventory methods are patched and movement is intercepted when needed. This fixes desyncs with other mods greatly and removes a lot of specific compatibility code. While this doesn't make all quick stack mods compatible, it simplifies compatibility for the most ones.
- Added compatibility with [Quick Stack Store Sort Trash Restock](https://valheim.thunderstore.io/package/Goldenrevolver/Quick_Stack_Store_Sort_Trash_Restock) by Goldenrevolver
- Added compatibility with [DepositAnywhere](https://valheim.thunderstore.io/package/Lookenpeepers/DepositAnywhere) by Lookenpeepers
- Added debug files (MultiUserChest.dll.mdb) to the release, this will make finding future issues easier
- Changed that the mod is now always required on the server and the version check matches the exact version instead of only minor releases
- Fixed an item duplication bug when a player moved an item into a chest and fast enough elsewhere
- Fixed an item duplication bug with the take all action
- Fixed the last item stack could not be fast moved if a stack has space left
- Fixed a compatibility issue with EpicLoot while picking up the tombstone
- Fixed a compatibility issue with Equipment and Quick Slots when trying to throw an item out of a a quick slot

0.3.1
- Improved mod compatibility by loading items into a temporary inventory first
- Fixed item stack size modification from V+ was causing issues
- Changed chest inventory reload to be called after vanilla, this makes the inventory not break completely with OdinsQOL AutoStore feature. Nevertheless, both AutoStore and MUC are incompatible at this time

0.3.0
- Updated for Valheim 0.212.7 (Mistlands)
- Fixed that an item will not be deselected if the underlying chest reloads
- Fixed that a slot could stay blocked if the owner of the chest changes

0.2.4
- Fixed items from EIDF were not loaded correctly when fast moving them inside shared chests. They had to be picked up to show the extended item data again
- Improved performance of adding item to inventories by avoiding costly vanilla code

0.2.3
- Fixed item loss in high latency situations. This could occur when the receiving slot was occupied before the item was fully transferred. If items cannot be delivered, they are now dropped to the ground

0.2.2
- Added compatibility with [SmartContainers](https://www.nexusmods.com/valheim/mods/332)

0.2.1
- Fixed an error that could occur when an item was moved while the player is spawning

0.2.0
- Added ability to making synced non-player interactions on chests possible. This will be the base for other mods I am working on
- Added BepInEx incompatibility for conflicting mods to prevent item issues. This means Multi User Chest will not start if any of the incompatible mods are installed
- Added Jotunn dependency for utilities
- Added enforcement of mod version if the mod is installed on the server
- Removed Container.IsInUse() patch to improve mod compatibility
- Removed custom file logger
- General improvements

0.1.6
- Fixed errors with Jewelcrafting

0.1.5
- Fixed the compatibility patch with Valheim Simple Auto Sort was always active, resulting in a harmony error

0.1.4
- Added compatibility with [Valheim Simple Auto Sort](https://www.nexusmods.com/valheim/mods/1824)

0.1.3
- Sync visual chest opening with other players
- Added compatibility with [QuickDeposit](https://valheim.thunderstore.io/package/MaGic/Quick_Deposit/) and [QuickStack](https://valheim.thunderstore.io/package/damnsneaker/QuickStack/)

0.1.2
- Fixed item loss with EAQS quick slots on death, when inventory was full. Only fixes the interaction with both mods, not changing EAQS itself

0.1.1
- Fixed pickup items could be deleted when player inventory is full and item was switched or rejected by chest
- Fixed error with while placing ItemDrawers
- Removed alpha status of this mod

0.1.0
- Release
