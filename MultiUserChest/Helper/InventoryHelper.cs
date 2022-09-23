using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using MultiUserChest.Patches.Compatibility;
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
            bool equipped = pkg.ReadBool();
            int quality = pkg.ReadInt();
            int variant = pkg.ReadInt();
            long crafterID = pkg.ReadLong();
            string crafterName = pkg.ReadString();

            ItemDrop.ItemData itemData = GetItemDataFromObjectDB(name);

            if (itemData == null) {
                return null;
            }

            itemData.m_stack = Mathf.Min(stack, itemData.m_shared.m_maxStackSize);
            itemData.m_durability = durability;
            itemData.m_equiped = equipped;
            itemData.m_quality = quality;
            itemData.m_variant = variant;
            itemData.m_crafterID = crafterID;
            itemData.m_crafterName = crafterName;
            itemData.m_gridPos = new Vector2i(pos.x, pos.y);

            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.extendeditemdataframework")) {
                return ExtendedItemDataFramework.CreateExtendedItemData(itemData);
            }

            return itemData;
        }

        public static void WriteItemToPackage(ItemDrop.ItemData itemData, ZPackage pkg) {
            pkg.Write(itemData != null);

            if (itemData == null) {
                return;
            }

            if (itemData.m_dropPrefab == null) {
                Log.LogWarning("Item missing prefab " + itemData.m_shared.m_name);
                pkg.Write(itemData.m_shared.m_name);
            } else {
                pkg.Write(itemData.m_dropPrefab.name);
            }

            pkg.Write(itemData.m_stack);
            pkg.Write(itemData.m_durability);
            pkg.Write(itemData.m_gridPos);
            pkg.Write(itemData.m_equiped);
            pkg.Write(itemData.m_quality);
            pkg.Write(itemData.m_variant);
            pkg.Write(itemData.m_crafterID);
            pkg.Write(itemData.m_crafterName);
        }

        public static ItemDrop.ItemData GetItemDataFromObjectDB(string name) {
            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode());

            if (itemPrefab == null) {
                Log.LogWarning("Failed to find item prefab " + name);
                return null;
            }

            ItemDrop component = itemPrefab.GetComponent<ItemDrop>();

            if (component == null) {
                Log.LogWarning("Missing itemdrop in " + name);
                return null;
            }

            return new ItemDrop.ItemData {
                m_shared = component.m_itemData.m_shared,
                m_dropPrefab = itemPrefab,
            };
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

        public static void PrintItem(ItemDrop.ItemData itemData) {
#if FULL_DEBUG
            if (itemData == null) {
                return;
            }

            Log.LogDebug($"    drop name: {(itemData.m_dropPrefab != null ? itemData.m_dropPrefab.name : "null!!!")}");
            Log.LogDebug($"    shared name: {itemData.m_shared.m_name}");
            Log.LogDebug($"    stack: {itemData.m_stack}");
#endif
        }
    }
}
