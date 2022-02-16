ModName="MultiUserChest"
ModPath="NoChestBlock"

# function for xml reading
read_dom () {
    local IFS=\>
    read -d \< ENTITY CONTENT
}

# read install folder from environment
while read_dom; do
	if [[ $ENTITY = "VALHEIM_INSTALL" ]]; then
		VALHEIM_INSTALL=$CONTENT
	fi
	if [[ $ENTITY = "R2MODMAN_INSTALL" ]]; then
		R2MODMAN_INSTALL=$CONTENT
	fi
	if [[ $ENTITY = "USE_R2MODMAN_AS_DEPLOY_FOLDER" ]]; then
		USE_R2MODMAN_AS_DEPLOY_FOLDER=$CONTENT
	fi
done < Environment.props

# set ModDir
if $USE_R2MODMAN_AS_DEPLOY_FOLDER; then
  BepInExFolder="$R2MODMAN_INSTALL/BepInEx"
else
	BepInExFolder="$VALHEIM_INSTALL/BepInEx"
fi

PluginFolder="$BepInExFolder/plugins"
ModDir="$PluginFolder/$ModName"

echo Coping...
echo "$ModDir"

# copy content
mkdir -p "$ModDir"
cp "$ModPath/bin/Debug/$ModName.dll" "$ModDir"
cp "$ModPath/bin/Debug/$ModName.pdb" "$ModDir"
cp README.md "$ModDir"
cp manifest.json "$ModDir"
cp icon.png "$ModDir"

# make zip files
cd "$ModDir" || exit

[ -f "$ModName.zip" ] && rm "$ModName.zip"
[ -f "$ModName-Nexus.zip" ] && rm "$ModName-Nexus.zip"

mkdir -p plugins/"$ModName"
cp "$ModName.dll" plugins/"$ModName"

zip "$ModName.zip" "$ModName.dll" README.md manifest.json icon.png
zip -r "$ModName-Nexus.zip" plugins

rm -r plugins
