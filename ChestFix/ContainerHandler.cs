using System;
using UnityEngine;

namespace ChestFix {
    public static class ContainerHandler {
        public static void RPC_RequestItemMove(this Container container, long sender, ZPackage package) {
            Log.LogInfo("RPC_RequestItemMove");

            Vector2i fromPos = package.ReadVector2i();
            Vector2i toPos = package.ReadVector2i();
            int dragAmount = package.ReadInt();

            ItemDrop.ItemData from = container.GetInventory().GetItemAt(fromPos.x, fromPos.y);

            if (from == null) {
                Log.LogInfo("from == null: true");
                container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", false);
                return;
            }

            if (InventoryHelper.MoveItem(container.GetInventory(), from, dragAmount, toPos)) {
                ZDOMan.instance.ForceSendZDO(container.m_nview.GetZDO().m_uid);
                container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", true);
                return;
            }

            container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", false);
        }

        public static void RPC_RequestItemAdd(this Container container, long sender, ZPackage package) {
            Log.LogInfo("RPC_RequestItemAdd");

            long playerId = package.ReadLong();
            Vector2i fromInventory = package.ReadVector2i();
            Vector2i toContainer = package.ReadVector2i();
            int dragAmount = package.ReadInt();

            Log.LogInfo($"playerId : {playerId}");
            Log.LogInfo($"fromInventory : {fromInventory}");
            Log.LogInfo($"toContainer : {toContainer}");
            Log.LogInfo($"dragAmount : {dragAmount}");

            Player player = Player.GetPlayer(playerId);
            Log.LogInfo($"player : {player.GetPlayerName()}");
            Log.LogInfo($"player : {player.GetInventory().GetEmptySlots()}");

            int amount = dragAmount;

            ItemDrop.ItemData prevItem = container.GetInventory().GetItemAt(toContainer.x, toContainer.y);
            if (prevItem != null) {
                amount = Mathf.Min(prevItem.m_shared.m_maxStackSize - prevItem.m_stack, dragAmount);
            }

            bool added = InventoryHelper.LoadItemIntoInventory(package, container.GetInventory(), toContainer, amount);

            container.m_nview.InvokeRPC(sender, "RequestItemAddResponse", added, amount);
        }

        public static ZPackage RPC_RequestItemRemove(this Inventory inventory, long sender, ZPackage package) {
            ZPackage response = new ZPackage();

            Log.LogInfo("RPC_RequestItemRemove");

            // read package data
            Vector2i fromContainer = package.ReadVector2i();
            Vector2i toInventory = package.ReadVector2i();
            int dragAmount = package.ReadInt();
            bool hasSwitchItem = package.ReadBool();

            ItemDrop.ItemData from = inventory.GetItemAt(fromContainer.x, fromContainer.y);

            if (from == null) {
                Log.LogInfo("from is null");
                response.Write(false); // not removed
                response.Write(0);
                response.Write(false); // not switched
                return response;
            }

            int removedAmount = Mathf.Min(from.m_stack, dragAmount);
            bool removed = inventory.RemoveItem(from, dragAmount);

            bool switched = false;

            if (hasSwitchItem) {
                switched = InventoryHelper.LoadItemIntoInventory(package, inventory, fromContainer, -1);
                ItemDrop.ItemData addedItem = inventory.GetItemAt(fromContainer.x, fromContainer.y);

                if (InventoryHelper.IsSameItem(from, addedItem)) {
                    switched = false;
                    int stackSize = from.m_shared.m_maxStackSize;
                    removedAmount = Mathf.Min(removedAmount, stackSize - addedItem.m_stack);
                    inventory.RemoveItem(addedItem, removedAmount);
                }
            }

            response.Write(removed);
            response.Write(removedAmount);
            response.Write(switched);

            return response;
        }
    }
}
