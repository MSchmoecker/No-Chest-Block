# Changelog

0.5.10
- Uploaded without debugging context, which has slowed down the mod a lot

0.5.9
- Fixed issues with ships and carts caused by the 0.5.8 update

0.5.8
- Fixed issues when a player interacts with a chest while the owning player is teleporting away

0.5.7
- Fixed type warning about ItemDrawer when it's not installed

0.5.6
- Fixed a conflict with [Backpacks](https://valheim.thunderstore.io/package/Smoothbrain/Backpacks/) where dropping an item from a backpack throws an error
- Fixed compatibility with [ItemDrawers](https://valheim.thunderstore.io/package/makail/ItemDrawers/) for dependent mods like [ItemHopper](https://valheim.thunderstore.io/package/MSchmoecker/ItemHopper/) where stack sizes were wrong and the drawer did not update correctly
- Fixed a potential item loss issue when another mod patches AddItem() to reject an item inconsistently between clients
- Removed some vanilla logging when adding an item to an inventory, to avoid clutter and slightly improve performance when many actions are done

0.5.5
- Compatible with Valheim version 0.217.22, not working with an older version
- Updated and compiled for BepInExPack 5.4.2200

0.5.4
- Fixed equipped items appear to be un-equipped when item preview shows an item in the player inventory, resulting in equipped items background flashing

0.5.3
- Updated for Valheim 0.217.14 (Hildir's Request), not compatible with older versions

0.5.2
- Removed [QuickStack](https://valheim.thunderstore.io/package/damnsneaker/QuickStack) compatibility as the mod isn't updated for Valheim and threw a MUC error. The mods will no longer start together

0.5.1
- Updated for Valheim 0.216.9, not compatible with older versions

0.5.0
- Added item preview. This visually shows changes before all messages have been processed and improves the flow of interaction within a shared chest
- Changed internal messages and made general code improvements
- Fixed an issue where items could be lost when a player removes an item from a shared chest and the stack size has changed in the meantime

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
