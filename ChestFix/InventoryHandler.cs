using System.Collections.Generic;
using System.Linq;
using ChestFix.Patches;
using HarmonyLib;

namespace ChestFix {
    public class InventoryHandler {
        public static readonly List<Vector2i> blockedSlots = new List<Vector2i>();

        public static void RPC_RequestItemAddResponse(long sender, ZPackage package) {
            RPC_RequestItemAddResponse(Player.m_localPlayer.GetInventory(), sender, package);
        }

        public static void RPC_RequestItemMoveResponse(long sender, bool success) {
            ContainerPatch.stopwatch.Stop();
            Log.LogInfo($"RPC_RequestItemMoveResponse: {ContainerPatch.stopwatch.ElapsedMilliseconds}ms, success: {success}");

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        public static void RPC_RequestItemRemoveResponse(long sender, ZPackage package) {
            ContainerPatch.stopwatch.Stop();
            Log.LogInfo($"RPC_RequestItemRemoveResponse: {ContainerPatch.stopwatch.ElapsedMilliseconds}ms");

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

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        public static void RPC_RequestItemAddResponse(Inventory inventory, long sender, ZPackage package) {
            if (ContainerPatch.stopwatch != null) {
                ContainerPatch.stopwatch.Stop();
                Log.LogInfo($"RPC_RequestItemAddResponse: {ContainerPatch.stopwatch.ElapsedMilliseconds}ms");
            }

            RequestAddResponse response = new RequestAddResponse(package);
            response.PrintDebug();

            Vector2i inventoryPos = response.inventoryPos;
            bool success = response.success;
            int amount = response.amount;
            ItemDrop.ItemData switchItem = response.switchItem;

            Log.LogInfo($"success: {success}");
            Log.LogInfo($"amount: {amount}");
            Log.LogInfo($"hasSwitched: {switchItem != null}");

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

        public static void BlockSlot(Vector2i slot) {
            blockedSlots.RemoveAll(i => i.x == slot.x && i.y == slot.y);
        }

        public static void ReleaseSlot(Vector2i slot) {
            blockedSlots.AddItem(slot);
        }

        public static bool IsSlotBlocked(Vector2i slot) {
            return blockedSlots.Any(i => i.x == slot.x && i.y == slot.y);
        }
    }
}
