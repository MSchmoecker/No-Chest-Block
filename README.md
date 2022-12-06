# Multi User Chest


## About
Allows multiple players to interact with the same chest at the same time.


## Installation
This mod requires [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) and [Jötunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/).
Extract all files to `BepInEx/plugins/MultiUserChest`

All players need to install this mod.
It can be installed at a server to enforce the installation on all clients.

Please report all errors and desyncs if they occur.


## FAQ
- Who needs this installed?
  - Ever player, server is optional to enforce the installation.
- When two players move an item at the exact same time, will it be duplicated?
  - No, only one movement will be accepted.
- Can this be added to an existing game?
  - Yes, you can add and remove this mod anytime. Just make sure it is the same for all players.
- Does it work with other modded chests
  - Yes, most likely. But it can make problems when other mods change the behaviour of chests
- Does it work with mods that change inventory sizes
  - Yes
- Does it work with mods that add quick stacking / chest sorting
  - Depends on the mod, see [Incompatible Mods](#confirmed-incompatible-mods) and [Compatible Mods](#compatible-mods). If it not listed there, assume it doesn't work


## Confirmed Incompatible Mods
These mods cause desync with Multi User Chest and will not startup together
- QuickStore https://www.nexusmods.com/valheim/mods/1595
- DepositAnywhere https://valheim.thunderstore.io/package/Lookenpeepers/DepositAnywhere
- SimpleSort https://www.nexusmods.com/valheim/mods/584


## Compatible Mods
Some of those mods are patched by Multi User Chest to make them work together.
The version number indicates the tested version of the other mod, a different one may also work but isn't confirmed.
- SmartContainers 1.6.3 https://www.nexusmods.com/valheim/mods/332
- QuickDeposit 1.0.1 https://valheim.thunderstore.io/package/MaGic/Quick_Deposit
- QuickStack 0.6.6 https://valheim.thunderstore.io/package/damnsneaker/QuickStack
- Valheim Simple Auto Sort 1.0.7 https://www.nexusmods.com/valheim/mods/1824


## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/MultiUserChest/
- Nexus: https://www.nexusmods.com/valheim/mods/1766
- Github: https://github.com/MSchmoecker/No-Chest-Block
- Discord: Margmas#9562. Feel free to DM or ping me in the [Jötunn discord](https://discord.gg/DdUt6g7gyA)


## Development
See [contributing](https://github.com/MSchmoecker/No-Chest-Block/blob/master/CONTRIBUTING.md).

## Changelog
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
