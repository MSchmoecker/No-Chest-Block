using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiUserChest {
    public static class InventoryHelper {
        public delegate void MoveAction(Inventory from, Inventory to);

        public static ItemDrop.ItemData LoadItemFromPackage(ZPackage pkg) {
            bool hasItem = pkg.ReadBool();

            if (!hasItem) {
                return null;
            }

            string name = pkg.ReadString();
            int stack = pkg.ReadInt();
            float durability = pkg.ReadSingle();
            Vector2i pos = pkg.ReadVector2i();
            int quality = pkg.ReadInt();
            int variant = pkg.ReadInt();
            long crafterID = pkg.ReadLong();
            string crafterName = pkg.ReadString();

            Dictionary<string, string> customData = new Dictionary<string, string>();
            int customDataCount = pkg.ReadInt();

            for (int i = 0; i < customDataCount; i++) {
                string key = pkg.ReadString();
                string value = pkg.ReadString();
                customData[key] = value;
            }

            // invoke vanilla method to add a new item into the inventory system
            // while not as performant as adding creating a ItemDrop manually, this improves compatibility with other mods
            Inventory tempInventory = new Inventory("tmp", null, pos.x + 1, pos.y + 1);
            tempInventory.AddItem(name, stack, durability, pos, false, quality, variant, crafterID, crafterName, customData);

            ItemDrop.ItemData itemData = tempInventory.GetItemAt(pos.x, pos.y);
            tempInventory.RemoveItem(itemData);

            return itemData;
        }

        public static void WriteItemToPackage(ItemDrop.ItemData itemData, ZPackage pkg) {
            pkg.Write(itemData != null);

            if (itemData == null) {
                return;
            }

            pkg.Write(itemData.PrefabName());
            pkg.Write(itemData.m_stack);
            pkg.Write(itemData.m_durability);
            pkg.Write(itemData.m_gridPos);
            pkg.Write(itemData.m_quality);
            pkg.Write(itemData.m_variant);
            pkg.Write(itemData.m_crafterID);
            pkg.Write(itemData.m_crafterName);

            pkg.Write(itemData.m_customData.Count);
            foreach (KeyValuePair<string, string> pair in itemData.m_customData) {
                pkg.Write(pair.Key);
                pkg.Write(pair.Value);
            }
        }

        public static string PrefabName(this ItemDrop.ItemData item) {
            if (item.m_dropPrefab != null) {
                return item.m_dropPrefab.name;
            }

            Log.LogWarning("Item missing prefab " + item.m_shared.m_name);
            return item.m_shared.m_name;
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

            if (itemAt == null) {
                return inventory.MoveItemToThis(inventory, item, amount, toPos.x, toPos.y);
            }

            if (IsSameItem(itemAt, item)) {
                // items can be stacked
                int stackAmount = Mathf.Min(amount, itemAt.m_shared.m_maxStackSize - itemAt.m_stack);
                return inventory.MoveItemToThis(inventory, item, stackAmount, toPos.x, toPos.y);
            }

            if (item.m_stack != amount) {
                return false;
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

        public static Vector2i FindEmptySlot(Inventory inventory, List<Vector2i> blockedSlots) {
            for (int y = 0; y < inventory.m_height; ++y) {
                for (int x = 0; x < inventory.m_width; ++x) {
                    if (inventory.GetItemAt(x, y) != null || blockedSlots.Contains(new Vector2i(x, y))) {
                        continue;
                    }

                    return new Vector2i(x, y);
                }
            }

            return new Vector2i(-1, -1);
        }

        public static List<ItemDrop.ItemData> GetAllMoveableItems(Inventory from, Inventory to, MoveAction moveAction) {
            Inventory fromCopy = CopyInventory(from);
            Inventory toCopy = CopyInventory(to);

            moveAction(fromCopy, toCopy);

            return CountMoved(from, fromCopy);
        }

        public static List<ItemDrop.ItemData> GetAllMoveableItems(Inventory from, Inventory to) {
            return GetAllMoveableItems(from, to, MoveAll);
        }

        public static void MoveAll(Inventory from, Inventory to) {
            to.MoveAll(from);
        }

        private static List<ItemDrop.ItemData> CountMoved(Inventory from, Inventory fromCopy) {
            List<ItemDrop.ItemData> moved = new List<ItemDrop.ItemData>();

            foreach (ItemDrop.ItemData originalItem in from.m_inventory) {
                ItemDrop.ItemData nowItem = fromCopy.GetItemAt(originalItem.m_gridPos.x, originalItem.m_gridPos.y);

                if (nowItem == null) {
                    moved.Add(originalItem);
                } else if (originalItem.m_stack > nowItem.m_stack) {
                    ItemDrop.ItemData clone = originalItem.Clone();
                    clone.m_stack = originalItem.m_stack - nowItem.m_stack;
                    moved.Add(clone);
                }
            }

            return moved;
        }

        public static Inventory CopyInventory(Inventory target) {
            return new Inventory(target.m_name, target.m_bkg, target.m_width, target.m_height) {
                m_inventory = new List<ItemDrop.ItemData>(target.GetAllItems().Select(x => x.Clone()))
            };
        }

        public static bool AddItemToInventory(this Inventory target, ItemDrop.ItemData item, int amount, Vector2i pos) {
            if (amount == 0) {
                return false;
            }

            ItemDrop.ItemData curItem = target.GetItemAt(pos.x, pos.y);

            if (curItem != null) {
                if (!IsSameItem(curItem, item)) {
                    return false;
                }

                if (curItem.m_stack + amount > curItem.m_shared.m_maxStackSize) {
                    return false;
                }
            }

            ItemDrop.ItemData clone = item.Clone();
            clone.m_stack = amount;

            return target.AddItem(clone, amount, pos.x, pos.y);
        }

        public static void PrintItem(string title, ItemDrop.ItemData itemData) {
#if FULL_DEBUG
            if (itemData == null) {
                Log.LogDebug($"  {title}: false");
                return;
            }

            Log.LogDebug($"  {title}: true");
            Log.LogDebug($"    drop name: {(itemData.m_dropPrefab != null ? itemData.m_dropPrefab.name : "null!!!")}");
            Log.LogDebug($"    shared name: {itemData.m_shared.m_name}");
            Log.LogDebug($"    stack: {itemData.m_stack}");
#endif
        }
    }
}
