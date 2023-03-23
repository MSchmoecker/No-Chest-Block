using BepInEx.Bootstrap;

namespace MultiUserChest {
    public static class OdinShip {
        public static bool IsOdinShipInstalled() {
            return Chainloader.PluginInfos.ContainsKey("marlthon.OdinShipPlus") || Chainloader.PluginInfos.ContainsKey("marlthon.OdinShip");
        }

        public static bool IsOdinShipContainer(this Container container) {
            return container.GetType().FullName == "OdinShip.ShipContainer";
        }
    }
}
