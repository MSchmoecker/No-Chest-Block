using System;
using System.Collections.Generic;

namespace MultiUserChest {
    public class InventoryBlock {
        private static readonly Dictionary<Inventory, InventoryBlock> Inventories = new Dictionary<Inventory, InventoryBlock>();

        public bool BlockAllSlots { get; set; }
        public bool BlockConsume { get; set; }

        private readonly Dictionary<Vector2i, int> blockedSlots = new Dictionary<Vector2i, int>();

        public static InventoryBlock Get(Inventory inventory) {
            if (Inventories.ContainsKey(inventory)) {
                return Inventories[inventory];
            }

            InventoryBlock block = new InventoryBlock();
            Inventories.Add(inventory, block);
            return block;
        }

        public void BlockSlot(Vector2i slot) {
            if (!CanBlockSlot(slot)) {
                return;
            }

            if (blockedSlots.ContainsKey(slot)) {
                blockedSlots[slot]++;
            } else {
                blockedSlots[slot] = 1;
            }
        }

        public void ReleaseSlot(Vector2i slot) {
            if (!CanBlockSlot(slot)) {
                return;
            }

            if (!blockedSlots.ContainsKey(slot)) {
                return;
            }

            blockedSlots[slot]--;

            if (blockedSlots[slot] <= 0) {
                blockedSlots.Remove(slot);
            }
        }

        public void ReleaseBlockedSlots() {
            blockedSlots.Clear();
            BlockAllSlots = false;
            BlockConsume = false;
        }

        public bool IsSlotBlocked(Vector2i slot) {
            return BlockConsume || BlockAllSlots || blockedSlots.ContainsKey(slot);
        }

        public bool IsAnySlotBlocked() {
            return BlockConsume || BlockAllSlots || blockedSlots.Count > 0;
        }

        private static bool CanBlockSlot(Vector2i slot) {
            return slot.x >= 0 && slot.y >= 0;
        }
    }
}
