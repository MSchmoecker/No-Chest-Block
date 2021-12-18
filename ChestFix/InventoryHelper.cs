namespace ChestFix {
    public static class InventoryHelper {
        public static bool LoadItemIntoInventory(ZPackage pkg, Inventory inventory, Vector2i pos, int amount) {
            string name = pkg.ReadString();
            int stack = pkg.ReadInt();
            float durability = pkg.ReadSingle();
            bool equipped = pkg.ReadBool();
            int quality = pkg.ReadInt();
            int variant = pkg.ReadInt();
            long crafterID = pkg.ReadLong();
            string crafterName = pkg.ReadString();

            if (name == string.Empty) {
                return false;
            }

            int actualAmount = amount >= 0 ? amount : stack;

            return inventory.AddItem(name, actualAmount, durability, pos, equipped, quality, variant, crafterID, crafterName);
        }

        public static void WriteItemToPackage(ItemDrop.ItemData itemData, ZPackage pkg) {
            if (itemData.m_dropPrefab == null) {
                ZLog.Log("Item missing prefab " + itemData.m_shared.m_name);
                pkg.Write("");
            } else {
                pkg.Write(itemData.m_dropPrefab.name);
            }

            pkg.Write(itemData.m_stack);
            pkg.Write(itemData.m_durability);
            pkg.Write(itemData.m_equiped);
            pkg.Write(itemData.m_quality);
            pkg.Write(itemData.m_variant);
            pkg.Write(itemData.m_crafterID);
            pkg.Write(itemData.m_crafterName);
        }
    }
}
