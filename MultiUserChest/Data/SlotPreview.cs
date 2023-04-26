using System.Collections.Generic;
using System.Linq;

namespace MultiUserChest {
    public class SlotPreview {
        private Inventory inventory;
        private Dictionary<Vector2i, List<Slot>> rawSlots = new Dictionary<Vector2i, List<Slot>>();

        public SlotPreview(Inventory inventory) {
            this.inventory = inventory;
        }

        public void Add(Vector2i pos, ItemDrop.ItemData item, int amount) {
            if (item == null || amount == 0) {
                return;
            }

            InitSlot(pos);
            rawSlots[pos].Add(new Slot(item, amount));
        }

        public void Add(Vector2i pos, ItemDrop.ItemData item) {
            Add(pos, item, item?.m_stack ?? 0);
        }

        public void Remove(Vector2i pos, ItemDrop.ItemData item) {
            Add(pos, item, -item?.m_stack ?? 0);
        }

        public void Remove(Vector2i pos, ItemDrop.ItemData item, int amount) {
            Add(pos, item, -amount);
        }

        public bool GetSlot(Vector2i pos, out ItemDrop.ItemData item) {
            if (!rawSlots.TryGetValue(pos, out List<Slot> slots)) {
                item = inventory.GetItemAt(pos.x, pos.y);
                return false;
            }

            IGrouping<string, Slot> group = slots.Where(i => i.item != null).GroupBy(i => i.item.PrefabName()).FirstOrDefault(i => i.Sum(s => s.amount) > 0);

            if (group == null) {
                item = null;
                return true;
            }

            item = group.First().item;
            item.m_stack = group.Sum(s => s.amount);
            item.m_gridPos = pos;
            return true;
        }

        private void InitSlot(Vector2i pos) {
            if (rawSlots.ContainsKey(pos)) {
                return;
            }

            ItemDrop.ItemData currentItem = inventory.GetItemAt(pos.x, pos.y);

            if (currentItem != null) {
                rawSlots[pos] = new List<Slot> { new Slot(currentItem, currentItem.m_stack) };
            } else {
                rawSlots[pos] = new List<Slot>();
            }
        }

        private class Slot {
            public ItemDrop.ItemData item;
            public int amount;

            public Slot(ItemDrop.ItemData item, int amount) {
                this.item = item?.Clone();
                this.amount = amount;
            }
        }

        public bool HasChanges() {
            return rawSlots.Count > 0;
        }
    }
}
