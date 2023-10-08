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
  - The server and every player who joins it.
    You will be disconnected if there is a version mismatch or the mod is not installed somewhere.

- If two players move an item at the exact same time, will it be duplicated?
  - No, only one move will be accepted.

- How is item duplication/loss prevented?
  - An item will never exist twice.
    When an interaction is made inside a shared chest, the item is packed into a message and sent to the player currently managing the chest.
    This player instance then decides whether or not to move the item and sends the result back to the player who made the interaction.

- Can this be added to an existing game?
  - Yes, you can add and remove this mod at any time.

- Will this mod work with other modded chests?
  - Yes, most likely.
    But it can may cause problems if other mods change the behaviour of chests.

- Will this mod work with mods that change inventory/chest sizes?
  - Yes.

- Does it work with mods that add quick stacking / chest sorting?
  - Depends on the mod, see [Incompatible Mods](#confirmed-incompatible-mods) and [Compatible Mods](#compatible-mods).
    If it's not listed there, assume it won't work.
  - Recommended is [Quick Stack Store Sort Trash Restock](https://valheim.thunderstore.io/package/Goldenrevolver/Quick_Stack_Store_Sort_Trash_Restock) by Goldenrevolver, as as it combines many quick stack features and provides explicit compatibility with Multi User Chest.


## Confirmed Incompatible Mods
These mods cause desync with Multi User Chest and will not startup together
- [QuickStore](https://www.nexusmods.com/valheim/mods/1595)
- [QuickStack](https://valheim.thunderstore.io/package/damnsneaker/QuickStack)
- [SimpleSort](https://www.nexusmods.com/valheim/mods/584)


## Compatible Mods
Here are some quick stack mods listed that are compatible with Multi User Chest.
The version number indicates the tested version of the other mod, a different one may also work but isn't confirmed.
- [Quick Stack Store Sort Trash Restock](https://valheim.thunderstore.io/package/Goldenrevolver/Quick_Stack_Store_Sort_Trash_Restock) 1.3.10 (recommended quick stack mod, a higher version very likely works too)
- [SmartContainers](https://www.nexusmods.com/valheim/mods/332) 1.6.4
- [QuickDeposit](https://valheim.thunderstore.io/package/MaGic/Quick_Deposit) 1.0.1
- [DepositAnywhere](https://valheim.thunderstore.io/package/Lookenpeepers/DepositAnywhere) 1.2.0
- [Valheim Simple Auto Sort](https://www.nexusmods.com/valheim/mods/1824) 1.0.7


## Links
- [Thunderstore](https://valheim.thunderstore.io/package/MSchmoecker/MultiUserChest/)
- [Nexus](https://www.nexusmods.com/valheim/mods/1766)
- [Github](https://github.com/MSchmoecker/No-Chest-Block)
- Discord: Margmas. Feel free to DM or ping me about feedback or questions, for example in the [Jötunn discord](https://discord.gg/DdUt6g7gyA)


## Development
See [contributing](https://github.com/MSchmoecker/No-Chest-Block/blob/master/CONTRIBUTING.md).


## Changelog
See [changelog](https://github.com/MSchmoecker/No-Chest-Block/blob/master/CHANGELOG.md).
