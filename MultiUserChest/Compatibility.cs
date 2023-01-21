using System.Collections.Generic;

namespace MultiUserChest {
    public static class Compatibility {
        public static bool IsExtendedInventory(this Inventory inventory, out List<Inventory> inventories) {
            if (inventory.IsType("ExtendedInventory") && inventory.HasField("_inventories")) {
                inventories = inventory.GetField<List<Inventory>>("_inventories");
                return true;
            }

            inventories = null;
            return false;
        }
    }
}
