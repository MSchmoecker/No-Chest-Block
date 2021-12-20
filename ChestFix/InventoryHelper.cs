using UnityEngine;

namespace ChestFix {
    public static class InventoryHelper {
        public static bool LoadItemIntoInventory(ZPackage pkg, Inventory inventory, Vector2i pos, int amount, int maxAmount) {
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
            if (maxAmount >= 0) {
                actualAmount = Mathf.Min(actualAmount, maxAmount);
            }

            return inventory.AddItem(name, actualAmount, durability, pos, equipped, quality, variant, crafterID, crafterName);
        }

        public static ItemDrop.ItemData LoadItemFromPackage(ZPackage pkg, Vector2i pos, bool nameHack = false) {
            string name = pkg.ReadString();
            int stack = pkg.ReadInt();
            float durability = pkg.ReadSingle();
            bool equipped = pkg.ReadBool();
            int quality = pkg.ReadInt();
            int variant = pkg.ReadInt();
            long crafterID = pkg.ReadLong();
            string crafterName = pkg.ReadString();

            if (name == string.Empty) {
                return null;
            }

            ItemDrop.ItemData itemData;
            GameObject gameObject = null;

            if (!nameHack) {
                GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
                if (itemPrefab == null) {
                    ZLog.Log("Failed to find item prefab " + name);
                    return null;
                }

                ZNetView.m_forceDisableInit = true;
                gameObject = Object.Instantiate(itemPrefab);
                ZNetView.m_forceDisableInit = false;
                ItemDrop component = gameObject.GetComponent<ItemDrop>();
                if (component == null) {
                    ZLog.Log("Missing itemdrop in " + name);
                    UnityEngine.Object.Destroy(gameObject);
                    return null;
                }

                itemData = component.m_itemData;
            } else {
                itemData = new ItemDrop.ItemData {
                    m_shared = new ItemDrop.ItemData.SharedData() {
                        m_name = name,
                        m_maxStackSize = 20,
                    }
                };
            }

            itemData.m_stack = Mathf.Min(stack, itemData.m_shared.m_maxStackSize);
            itemData.m_durability = durability;
            itemData.m_equiped = equipped;
            itemData.m_quality = quality;
            itemData.m_variant = variant;
            itemData.m_crafterID = crafterID;
            itemData.m_crafterName = crafterName;
            itemData.m_gridPos = new Vector2i(pos.x, pos.y);

            if (!nameHack) {
                Object.Destroy(gameObject);
            }

            return itemData;
        }

        public static void WriteItemToPackage(ItemDrop.ItemData itemData, ZPackage pkg, bool nameHack = false) {
            if (itemData.m_dropPrefab == null) {
                Log.LogInfo("Item missing prefab " + itemData.m_shared.m_name);
                pkg.Write(nameHack ? itemData.m_shared.m_name : "");
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

        public static bool MoveItem(Inventory inventory, ItemDrop.ItemData item, int amount, Vector2i toPos) {
            if (item == null) {
                // no item to move
                return false;
            }

            ItemDrop.ItemData itemAt = inventory.GetItemAt(toPos.x, toPos.y);

            if (itemAt == item) {
                // moved onto exact same item
                return true;
            }

            if (itemAt == null || IsSameItem(itemAt, item)) {
                // items can be stacked
                return inventory.MoveItemToThis(inventory, item, amount, toPos.x, toPos.y);
            }

            // items must be switched
            inventory.RemoveItem(item);
            inventory.MoveItemToThis(inventory, itemAt, itemAt.m_stack, item.m_gridPos.x, item.m_gridPos.y);
            inventory.MoveItemToThis(inventory, item, amount, toPos.x, toPos.y);
            return true;
        }

        public static bool IsSameItem(ItemDrop.ItemData itemA, ItemDrop.ItemData itemB) {
            return itemA.m_shared.m_name == itemB.m_shared.m_name &&
                   (itemA.m_shared.m_maxQuality <= 1 || itemB.m_quality == itemA.m_quality) &&
                   itemA.m_shared.m_maxStackSize != 1;
        }
    }
}
