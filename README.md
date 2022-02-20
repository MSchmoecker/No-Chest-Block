# Multi User Chest


## About
Allows multiple players to interact with the same chest at the same time


## Installation
This mod requires BepInEx.\
Extract the content of `MultiUserChest` into the `BepInEx/plugins` folder.

All player need to install this mod, it is not needed at a server. Otherwise items may be deleted.\
It is highly recommended to force the installation.

I have tested this mod to my best ability but cannot ensure there all edge cases covered, especially with other mods.
Please report all errors and desyncs that occur.


## FAQ
- Who needs this installed?
  - Ever player, server is not needed.
- When two players move an item at the exact same time, will it be duplicated?
  - No, only one movement will be accepted.
- Can this be added to an existing game?
  - Yes, you can add and remove this mod anytime. Just make sure it is the same for all players.
- Does it work with other modded chests
  - Yes, most likely. But it can make problems when other mods change the behaviour of chests
- Does it work with mods that change inventory sizes
  - Yes
- Does it work with mods that add quick stacking / chest sorting
  - No

## Confirmed Incompatible Mods
These mods cause desync with Multi User Chest and should not be used together
- QuickDeposit https://valheim.thunderstore.io/package/MaGic/Quick_Deposit/
- QickStore https://valheim.thunderstore.io/package/fenrir0054/QuickStore/
- QuckStack https://valheim.thunderstore.io/package/damnsneaker/QuickStack/
- SmartContainers https://valheim.thunderstore.io/package/ZOR/SmartContainers/
- DepositAnywhere https://valheim.thunderstore.io/package/Lookenpeepers/DepositAnywhere/
- SimpleSort https://valheim.thunderstore.io/package/Nexus/SimpleSort/


## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/MultiUserChest/
- Nexus: https://www.nexusmods.com/valheim/mods/1766
- Github: https://github.com/MSchmoecker/No-Chest-Block
- Discord: Margmas#9562


## Development
BepInEx must be setup at manual or with r2modman/Thunderstore Mod Manager.

Create a file called `Environment.props` inside the project root.
Copy the example and change the Valheim install path to your location.
If you use r2modman/Tunderstore Mod Manager you can set the path too, but this is optional.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <!-- Needs to be your path to the base Valheim folder -->
        <VALHEIM_INSTALL>E:\Programme\Steam\steamapps\common\Valheim</VALHEIM_INSTALL>
        <!-- Optional, needs to be the path to a r2modmanPlus profile folder -->
        <R2MODMAN_INSTALL>C:\Users\[user]\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\Develop</R2MODMAN_INSTALL>
        <USE_R2MODMAN_AS_DEPLOY_FOLDER>false</USE_R2MODMAN_AS_DEPLOY_FOLDER>
    </PropertyGroup>
</Project>
```

## Changelog
0.1.1
- Fixed pickup items could be deleted when player inventory is full and item was switched or rejected by chest
- Fixed error with while placing ItemDrawers
- Removed alpha status of this mod

0.1.0
- Release
