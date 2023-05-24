using System.Collections.Generic;
using System.Linq;

namespace MultiUserChest {
    public class SlotPreview {
        private Inventory originalInventory;
        private Inventory inventory;
        private bool hasChanges;

        public SlotPreview(Inventory inventory) {
            originalInventory = inventory;
            this.inventory = InventoryHelper.CopyInventory(inventory);
        }

        public void Add(Vector2i pos, ItemDrop.ItemData item, int amount) {
            if (item == null || amount == 0) {
                return;
            }

            ItemDrop.ItemData toAdd = item.Clone();
            toAdd.m_stack = amount;

            if (pos.x >= 0 && pos.y >= 0) {
                inventory.AddItem(toAdd, amount, pos.x, pos.y);
            } else {
                inventory.AddItem(toAdd);
            }

            hasChanges = true;
        }

        public void Add(Vector2i pos, ItemDrop.ItemData item) {
            Add(pos, item, item?.m_stack ?? 0);
        }

        public void Remove(Vector2i pos, ItemDrop.ItemData item) {
            Remove(pos, item, item?.m_stack ?? 0);
        }

        public void Remove(Vector2i pos, ItemDrop.ItemData item, int amount) {
            if (item == null || amount == 0) {
                return;
            }

            ItemDrop.ItemData toRemove = inventory.GetItemAt(pos.x, pos.y);

            if (toRemove == null) {
                return;
            }

            inventory.RemoveItem(inventory.GetItemAt(pos.x, pos.y), amount);
            hasChanges = true;
        }

        public bool GetSlot(Vector2i pos, out ItemDrop.ItemData item) {
            ItemDrop.ItemData originalItem = originalInventory.GetItemAt(pos.x, pos.y);
            item = inventory.GetItemAt(pos.x, pos.y);

            if (originalItem == null && item == null) {
                return false;
            }

            if (originalItem == null || item == null) {
                return true;
            }

            bool changed = !InventoryHelper.IsSameItem(originalItem, item) || originalItem.m_stack != item.m_stack;
            return changed;
        }

        public bool HasChanges() {
            return hasChanges;
        }
    }
}
