using System;
using UnityEngine;
using VNEI;

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

        public static void RPC_RequestItemRemove(this Container container, long sender, ZPackage package) {
            Log.LogInfo("RPC_RequestItemRemove");

            Vector2i fromContainer = package.ReadVector2i();
            Vector2i toInventory = package.ReadVector2i();
            int dragAmount = package.ReadInt();
            bool hasSwitchItem = package.ReadBool();

            ItemDrop.ItemData from = container.GetInventory().GetItemAt(fromContainer.x, fromContainer.y);

            if (from == null) {
                Log.LogInfo("from == null: true");
                container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", false);
                return;
            }

            bool added = false;
            if (hasSwitchItem) {
                added = InventoryHelper.LoadItemIntoInventory(package, container.GetInventory(), fromContainer, -1);
            }

            int removedAmount = Mathf.Min(from.m_stack, dragAmount);
            bool removed = container.GetInventory().RemoveItem(from, dragAmount);
            container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", removed, removedAmount, added);
        }
    }
}
