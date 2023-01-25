## Development
BepInEx must be setup at manual or with r2modman/Thunderstore Mod Manager.
Jötunn must be installed.

Create a file called `Environment.props` inside the project root.
Copy the example and change the Valheim install path to your location.
If you use r2modman/Tunderstore Mod Manager you can set the path too, but this is optional.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <!-- Needs to be your path to the base Valheim folder -->
        <VALHEIM_INSTALL>E:\Programme\Steam\steamapps\common\Valheim</VALHEIM_INSTALL>

        <!-- Optional, must to be the path to a r2modmanPlus/TMM profile folder -->
        <R2MODMAN_INSTALL>C:\Users\[user]\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\Develop</R2MODMAN_INSTALL>

        <!-- Optional, deployment destination folder (this base folder must already exists) -->
        <DEPLOY_FOLDER>C:/Users/[user]/AppData/Roaming/r2modmanPlus-local/Valheim/profiles/Develop</DEPLOY_FOLDER>
    </PropertyGroup>
</Project>
```
