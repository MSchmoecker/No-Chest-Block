using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NoChestBlock.Patches;

namespace NoChestBlock {
    public class InventoryHandler {
        public static readonly List<Vector2i> blockedSlots = new List<Vector2i>();
        public static bool blockConsume;
        public static bool blockAllSlots;

        public static void RPC_RequestItemAddResponse(long sender, ZPackage package) {
            Timer.Stop("RPC_RequestItemAddResponse");
            RPC_RequestItemAddResponse(Player.m_localPlayer.GetInventory(), sender, package);
        }

        public static void RPC_RequestItemMoveResponse(long sender, bool success) {
            Timer.Stop("RPC_RequestItemMoveResponse");
            InventoryGui.instance.SetupDragItem(null, null, 0);
            Log.LogInfo($"RequestItemMoveResponse:");
            Log.LogInfo($"\tsuccess: {success}");
        }

        public static void RPC_RequestTakeAllItemsResponse(long sender, ZPackage package) {
            Timer.Stop("RPC_RequestTakeAllItemsResponse");
            RPC_RequestTakeAllItemsResponse(Player.m_localPlayer.GetInventory(), sender, package);
        }

        public static void RPC_RequestItemRemoveResponse(long sender, ZPackage package) {
            Timer.Stop("RPC_RequestItemRemoveResponse");

            RequestRemoveResponse response = new RequestRemoveResponse(package);
            response.PrintDebug();

            bool success = response.success;
            int amount = response.amount;
            bool hasSwitched = response.hasSwitched;
            Vector2i inventoryPos = response.inventoryPos;
            ItemDrop.ItemData responseItem = response.responseItem;

            Inventory playerInv = Player.m_localPlayer.GetInventory();

            ReleaseSlot(inventoryPos);

            if (success) {
                if (hasSwitched) {
                    ItemDrop.ItemData atSlot = playerInv.GetItemAt(inventoryPos.x, inventoryPos.y);
                    playerInv.RemoveItem(atSlot);
                }

                playerInv.AddItem(responseItem, amount, inventoryPos.x, inventoryPos.y);
            }

            if (InventoryGui.instance != null) {
                InventoryGui.instance.SetupDragItem(null, null, 0);
            }
        }

        public static void RPC_RequestItemAddResponse(Inventory inventory, long sender, ZPackage package) {
            RequestAddResponse response = new RequestAddResponse(package);
            response.PrintDebug();

            Vector2i inventoryPos = response.inventoryPos;
            bool success = response.success;
            int amount = response.amount;
            ItemDrop.ItemData switchItem = response.switchItem;

            ReleaseSlot(inventoryPos);

            // if (success) {
            //     ItemDrop.ItemData toRemove = inventory.GetItemAt(inventoryPos.x, inventoryPos.y);
            //
            //     if (toRemove != null) {
            //         inventory.RemoveItem(toRemove, amount);
            //     }
            // } 

            if (switchItem != null) {
                inventory.AddItem(switchItem, switchItem.m_stack, inventoryPos.x, inventoryPos.y);
            }

            if (InventoryGui.instance != null) {
                InventoryGui.instance.SetupDragItem(null, null, 0);
            }
        }

        public static void RPC_RequestItemConsumeResponse(long sender, ZPackage package) {
            Timer.Stop("RPC_RequestItemConsumeResponse");

            RequestConsumeResponse response = new RequestConsumeResponse(package);
            response.PrintDebug();

            Player player = Player.m_localPlayer;
            blockConsume = false;

            if (response.item == null) {
                return;
            }

            if (!player.CanConsumeItem(response.item)) {
                return;
            }

            if (response.item.m_shared.m_consumeStatusEffect) {
                player.m_seman.AddStatusEffect(response.item.m_shared.m_consumeStatusEffect, true);
            }

            if (response.item.m_shared.m_food > 0.0) {
                player.EatFood(response.item);
            }
        }

        private static void RPC_RequestTakeAllItemsResponse(Inventory inventory, long sender, ZPackage package) {
            blockAllSlots = false;

            RequestTakeAll response = new RequestTakeAll(package);

            foreach (ItemDrop.ItemData item in response.items) {
                inventory.AddItem(item);
            }
        }

        public static void BlockSlot(Vector2i slot) {
            blockedSlots.Add(slot);
        }

        public static void ReleaseSlot(Vector2i slot) {
            blockedSlots.RemoveAll(i => i.x == slot.x && i.y == slot.y);
        }

        public static bool IsSlotBlocked(Vector2i slot) {
            return !blockAllSlots && blockedSlots.Contains(slot);
        }
    }
}
