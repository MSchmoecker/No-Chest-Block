using System;
using System.Collections.Generic;

namespace MultiUserChest {
    public class InventoryBlock {
        private static readonly Dictionary<Inventory, InventoryBlock> Inventories = new Dictionary<Inventory, InventoryBlock>();

        public bool BlockAllSlots { get; set; }
        public bool BlockConsume { get; set; }

        public Dictionary<Vector2i, int> BlockedSlots { get; } = new Dictionary<Vector2i, int>();

        public static InventoryBlock Get(Inventory inventory) {
            if (Inventories.TryGetValue(inventory, out InventoryBlock existingBlock)) {
                return existingBlock;
            }

            InventoryBlock block = new InventoryBlock();
            Inventories.Add(inventory, block);
            return block;
        }

        public void BlockSlot(Vector2i slot) {
            if (!CanBlockSlot(slot)) {
                return;
            }

            if (BlockedSlots.ContainsKey(slot)) {
                BlockedSlots[slot]++;
            } else {
                BlockedSlots[slot] = 1;
            }
        }

        public void ReleaseSlot(Vector2i slot) {
            if (!CanBlockSlot(slot)) {
                return;
            }

            if (!BlockedSlots.ContainsKey(slot)) {
                return;
            }

            BlockedSlots[slot]--;

            if (BlockedSlots[slot] <= 0) {
                BlockedSlots.Remove(slot);
            }
        }

        public void ReleaseBlockedSlots() {
            BlockedSlots.Clear();
            BlockAllSlots = false;
            BlockConsume = false;
        }

        public bool IsSlotBlocked(Vector2i slot) {
            return BlockConsume || BlockAllSlots || BlockedSlots.ContainsKey(slot);
        }

        public bool IsAnySlotBlocked() {
            return BlockConsume || BlockAllSlots || BlockedSlots.Count > 0;
        }

        private static bool CanBlockSlot(Vector2i slot) {
            return slot.x >= 0 && slot.y >= 0;
        }
    }
}
