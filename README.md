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
See [changelog](CHANGELOG.md).
