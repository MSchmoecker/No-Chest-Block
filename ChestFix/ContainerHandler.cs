using System;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace ChestFix {
    public static class ContainerHandler {
        public static void RPC_RequestItemMove(this Container container, long sender, ZPackage package) {
            Logger.LogInfo("RPC_RequestItemMove");

            Vector2i fromPos = package.ReadVector2i();
            Vector2i toPos = package.ReadVector2i();
            int dragAmount = package.ReadInt();

            ItemDrop.ItemData from = container.GetInventory().GetItemAt(fromPos.x, fromPos.y);

            if (from == null) {
                Logger.LogInfo("from == null: true");
                container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", false);
                return;
            }

            if (MoveItem(container.GetInventory(), container.GetInventory(), from, dragAmount, toPos)) {
                ZDOMan.instance.ForceSendZDO(container.m_nview.GetZDO().m_uid);
                container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", true);
                return;
            }

            container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", false);
        }

        public static void RPC_RequestItemAdd(this Container container, long sender, ZPackage package) {
            Logger.LogInfo("RPC_RequestItemAdd");

            long playerId = package.ReadLong();
            Vector2i fromInventory = package.ReadVector2i();
            Vector2i toContainer = package.ReadVector2i();
            int dragAmount = package.ReadInt();

            Logger.LogInfo($"playerId : {playerId}");
            Logger.LogInfo($"fromInventory : {fromInventory}");
            Logger.LogInfo($"toContainer : {toContainer}");
            Logger.LogInfo($"dragAmount : {dragAmount}");

            Player player = Player.GetPlayer(playerId);
            Logger.LogInfo($"player : {player.GetPlayerName()}");
            Logger.LogInfo($"player : {player.GetInventory().GetEmptySlots()}");

            int amount = dragAmount;

            ItemDrop.ItemData prevItem = container.GetInventory().GetItemAt(toContainer.x, toContainer.y);
            if (prevItem != null) {
                amount = Mathf.Min(prevItem.m_shared.m_maxStackSize - prevItem.m_stack, dragAmount);
            }

            bool added = LoadItemIntoInventory(package, container.GetInventory(), toContainer, amount);

            container.m_nview.InvokeRPC(sender, "RequestItemAddResponse", added, amount);
        }

        public static void RPC_RequestItemRemove(this Container container, long sender, ZPackage package) {
            Logger.LogInfo("RPC_RequestItemRemove");

            long playerId = package.ReadLong();
            Vector2i fromContainer = package.ReadVector2i();
            Vector2i toInventory = package.ReadVector2i();
            int dragAmount = package.ReadInt();

            ItemDrop.ItemData from = container.GetInventory().GetItemAt(fromContainer.x, fromContainer.y);

            if (from == null) {
                Logger.LogInfo("from == null: true");
                container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", false);
                return;
            }

            int removedAmount = Mathf.Min(from.m_stack, dragAmount);
            bool removed = container.GetInventory().RemoveItem(from, dragAmount);
            container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", removed, removedAmount);
        }

        public static void RPC_RequestItemRemoveSwitch(this Container container, long sender, ZPackage package) {
            Logger.LogInfo("RPC_RequestItemRemoveSwitch");

            long playerId = package.ReadLong();
            Vector2i fromContainer = package.ReadVector2i();
            Vector2i toInventory = package.ReadVector2i();
            int dragAmount = package.ReadInt();

            ItemDrop.ItemData from = container.GetInventory().GetItemAt(fromContainer.x, fromContainer.y);

            if (from == null) {
                Logger.LogInfo("from == null: true");
                container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", false);
                return;
            }

            int removedAmount = Mathf.Min(from.m_stack, dragAmount);
            bool removed = container.GetInventory().RemoveItem(from, dragAmount);

            bool added = LoadItemIntoInventory(package, container.GetInventory(), fromContainer, -1);

            container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", removed, removedAmount);
        }

        private static bool MoveItem(Inventory fromInventory, Inventory toInventory, ItemDrop.ItemData item, int amount, Vector2i toPos) {
            ItemDrop.ItemData itemAt = fromInventory.GetItemAt(toPos.x, toPos.y);

            if (itemAt == item) {
                return true;
            }

            if (itemAt == null ||
                itemAt.m_shared.m_name == item.m_shared.m_name && (item.m_shared.m_maxQuality <= 1 || itemAt.m_quality == item.m_quality) &&
                itemAt.m_shared.m_maxStackSize != 1 || item.m_stack != amount) {
                return toInventory.MoveItemToThis(fromInventory, item, amount, toPos.x, toPos.y);
            }

            fromInventory.RemoveItem(item);
            toInventory.MoveItemToThis(fromInventory, itemAt, itemAt.m_stack, item.m_gridPos.x, item.m_gridPos.y);
            fromInventory.MoveItemToThis(toInventory, item, amount, toPos.x, toPos.y);
            return true;
        }

        private static bool LoadItemIntoInventory(ZPackage pkg, Inventory inventory, Vector2i pos, int amount) {
            string text = pkg.ReadString();
            int stack = pkg.ReadInt();
            float durability = pkg.ReadSingle();
            bool equiped = pkg.ReadBool();
            int quality = pkg.ReadInt();
            int variant = pkg.ReadInt();
            long crafterID = pkg.ReadLong();
            string crafterName = pkg.ReadString();

            if (text != "") {
                return inventory.AddItem(text, amount >= 0 ? amount : stack, durability, pos, equiped, quality, variant, crafterID, crafterName);
            }

            return false;
        }
    }
}
