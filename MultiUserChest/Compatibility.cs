using System.Collections.Generic;

namespace MultiUserChest {
    public static class Compatibility {
        private static bool IsExtendedInventory(this Inventory inventory, out List<Inventory> inventories) {
            if (inventory.IsType("ExtendedInventory") && inventory.HasField("_inventories")) {
                inventories = inventory.GetField<List<Inventory>>("_inventories");
                return true;
            }

            inventories = null;
            return false;
        }

        /// <summary>
        ///     Compatibility wrapper for EAQS ExtendedInventory
        /// </summary>
        /// <param name="inventory"></param>
        /// <returns></returns>
        public static List<Inventory> GetInventories(this Inventory inventory) {
            if (inventory.IsExtendedInventory(out List<Inventory> inventories)) {
                return inventories;
            }

            return new List<Inventory> {
                inventory,
            };
        }
    }
}
