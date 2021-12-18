using Jotunn;

namespace ChestFix {
    public static class ContainerHandler {
        public static void RPC_RequestItemMove(Container container, long sender, ZPackage package) {
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

        public static void RPC_RequestItemAdd(Container container, long sender, ZPackage package) {
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

            bool added = LoadItemIntoInventory(package, container.GetInventory(), toContainer, dragAmount);

            container.m_nview.InvokeRPC(sender, "RequestItemAddResponse", added);

            // ItemDrop.ItemData from = player.GetInventory().GetItemAt(fromInventory.x, fromInventory.y);
            //
            // if (from == null) {
            //     Logger.LogInfo("from == null: true");
            //     return;
            // }
            //
            // if (MoveItem(player.GetInventory(), container.GetInventory(), from, dragAmount, toContainer)) {
            //     ZDOMan.instance.ForceSendZDO(container.m_nview.GetZDO().m_uid);
            //     container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", true);
            //     return;
            // }
            //
            // container.m_nview.InvokeRPC(sender, "RequestItemAddResponse", false);
        }

        public static void RPC_RequestItemRemove(Container container, long sender, ZPackage package) {
            Logger.LogInfo("RPC_RequestItemRemove");

            long playerId = package.ReadLong();
            Vector2i fromContainer = package.ReadVector2i();
            Vector2i toInventory = package.ReadVector2i();
            int dragAmount = package.ReadInt();

            Player player = Player.GetPlayer(playerId);
            ItemDrop.ItemData from = container.GetInventory().GetItemAt(fromContainer.x, fromContainer.y);

            if (from == null) {
                Logger.LogInfo("from == null: true");
                container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", false);
                return;
            }

            bool removed = container.GetInventory().RemoveItem(from, dragAmount);
            container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", removed);
/*
            if (MoveItem(container.GetInventory(), player.GetInventory(), from, dragAmount, toInventory)) {
                ZDOMan.instance.ForceSendZDO(container.m_nview.GetZDO().m_uid);
                container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", true);
                return;
            }

            container.m_nview.InvokeRPC(sender, "RequestItemRemoveResponse", false);*/
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
                return inventory.AddItem(text, amount, durability, pos, equiped, quality, variant, crafterID, crafterName);
            }

            return false;
        }
    }
}
