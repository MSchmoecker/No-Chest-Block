using System.Collections.Generic;
using ChestFix.Patches;

namespace ChestFix {
    public class InventoryHandler {
        public static readonly List<Vector2i> blockedInventorySlots = new List<Vector2i>();

        public static void RPC_RequestItemMoveResponse(long sender, bool success) {
            ContainerPatch.stopwatch.Stop();
            Log.LogInfo($"RPC_RequestItemMoveResponse: {ContainerPatch.stopwatch.ElapsedMilliseconds}ms, success: {success}");

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        public static void RPC_RequestItemRemoveResponse(long sender, ZPackage package) {
            ContainerPatch.stopwatch.Stop();
            Log.LogInfo($"RPC_RequestItemRemoveResponse: {ContainerPatch.stopwatch.ElapsedMilliseconds}ms");

            bool success = package.ReadBool();
            int amount = package.ReadInt();
            bool hasSwitched = package.ReadBool();
            Vector2i inventoryPos = package.ReadVector2i();
            bool hasResponseItem = package.ReadBool();
            ItemDrop.ItemData responseItem = null;

            if (hasResponseItem) {
                responseItem = InventoryHelper.LoadItemFromPackage(package, inventoryPos);
            }

            Inventory playerInv = Player.m_localPlayer.GetInventory();

            if (blockedInventorySlots.Contains(inventoryPos)) {
                blockedInventorySlots.Remove(inventoryPos);
            }

            if (success) {
                if (hasSwitched) {
                    ItemDrop.ItemData atSlot = playerInv.GetItemAt(inventoryPos.x, inventoryPos.y);
                    playerInv.RemoveItem(atSlot);
                }

                playerInv.AddItem(responseItem, amount, inventoryPos.x, inventoryPos.y);
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        public static void RPC_RequestItemAddResponse(long sender, ZPackage package) {
            ContainerPatch.stopwatch.Stop();
            Log.LogInfo($"RPC_RequestItemAddResponse: {ContainerPatch.stopwatch.ElapsedMilliseconds}ms");

            Vector2i inventoryPos = package.ReadVector2i();
            bool success = package.ReadBool();
            int amount = package.ReadInt();
            bool hasSwitched = package.ReadBool();

            Log.LogInfo($"success: {success}");
            Log.LogInfo($"amount: {amount}");
            Log.LogInfo($"hasSwitched: {hasSwitched}");

            if (success) {
                ItemDrop.ItemData toRemove = Player.m_localPlayer.GetInventory().GetItemAt(inventoryPos.x, inventoryPos.y);
                Player.m_localPlayer.GetInventory().RemoveItem(toRemove, amount);

                if (hasSwitched) {
                    InventoryHelper.LoadItemIntoInventory(package, Player.m_localPlayer.GetInventory(), inventoryPos, -1, -1);
                }
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }
    }
}
