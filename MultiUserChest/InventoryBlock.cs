using System;
using System.Collections.Generic;

namespace MultiUserChest {
    public class InventoryBlock {
        private static readonly Dictionary<Inventory, InventoryBlock> Inventories = new Dictionary<Inventory, InventoryBlock>();

        private readonly Dictionary<Vector2i, int> blockedSlots = new Dictionary<Vector2i, int>();
        private bool blockConsume;
        private bool blockAllSlots;

        public static InventoryBlock Get(Inventory inventory) {
            if (Inventories.ContainsKey(inventory)) {
                return Inventories[inventory];
            }

            InventoryBlock block = new InventoryBlock();
            Inventories.Add(inventory, block);
            return block;
        }

        public void BlockSlot(Vector2i slot) {
            if (blockedSlots.ContainsKey(slot)) {
                blockedSlots[slot]++;
            } else {
                blockedSlots[slot] = 1;
            }
        }

        public void ReleaseSlot(Vector2i slot) {
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
        }

        public void BlockAllSlots(bool block) {
            blockAllSlots = block;
        }

        public void BlockConsume(bool block) {
            blockConsume = block;
        }

        public bool IsSlotBlocked(Vector2i slot) {
            return !blockAllSlots && blockedSlots.ContainsKey(slot);
        }

        public bool IsAnySlotBlocked() {
            return blockAllSlots || blockedSlots.Count > 0;
        }

        public bool IsConsumeBlocked() {
            return blockConsume;
        }
    }
}
