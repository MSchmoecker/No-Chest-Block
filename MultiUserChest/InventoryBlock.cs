using System;
using System.Collections.Generic;

namespace MultiUserChest {
    public class InventoryBlock {
        private static readonly Dictionary<Inventory, InventoryBlock> Inventories = new Dictionary<Inventory, InventoryBlock>();

        private readonly List<Vector2i> blockedSlots = new List<Vector2i>();
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
            blockedSlots.Add(slot);
        }

        public void ReleaseSlot(Vector2i slot) {
            blockedSlots.RemoveAll(i => i.x == slot.x && i.y == slot.y);
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
            return !blockAllSlots && blockedSlots.Contains(slot);
        }

        public bool IsAnySlotBlocked() {
            return blockAllSlots || blockedSlots.Count > 0;
        }

        public bool IsConsumeBlocked() {
            return blockConsume;
        }
    }
}
