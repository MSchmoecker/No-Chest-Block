using System.Collections.Generic;
using JetBrains.Annotations;

namespace NoChestBlock {
    public static class ContainerHandler {
        private const bool BypassSelfRouting = false;

        public static void TakeAll(Container container) {
            Inventory playerInventory = Player.m_localPlayer.GetInventory();
            List<ItemDrop.ItemData> wanted = InventoryHelper.GetAllMoveableItems(container.GetInventory(), playerInventory);

            InventoryHandler.blockAllSlots = true;

            RequestTakeAll request = new RequestTakeAll(wanted);
            Timer.Start(request);

            if (container != null && container.m_nview) {
                container.m_nview.InvokeRPC("RequestTakeAllItems", request.WriteToPackage());
            }
        }

        [UsedImplicitly]
        public static RequestAdd AddItemToChest(this Container containerTo, Container containerFrom, Vector2i from, Vector2i to, int dragAmount = 1, bool allowSwitch = false) {
            return AddItemToChest(containerTo, containerFrom.GetInventory(), containerFrom.m_nview.m_zdo.m_uid, from, to, dragAmount, allowSwitch);
        }

        public static RequestAdd AddItemToChest(this Container container, Inventory inventory, ZDOID sender, Vector2i from, Vector2i to, int dragAmount, bool allowSwitch) {
            ItemDrop.ItemData item = inventory.GetItemAt(from.x, from.y).Clone();
            RequestAdd request = new RequestAdd(to, dragAmount, item, inventory.m_name, allowSwitch, sender);
            inventory.RemoveItem(inventory.GetItemAt(from.x, from.y), dragAmount);

            if (BypassSelfRouting && container != null && container.m_nview && container.m_nview.IsValid() && container.m_nview.IsOwner()) {
                RequestAddResponse response = container.GetInventory().RequestItemAdd(request);
                InventoryHandler.RPC_RequestItemAddResponse(inventory, response);
                return null;
            }

            InventoryHandler.BlockSlot(from);

            if (container != null && container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemAdd", request.WriteToPackage());
            }

            return request;
        }

        [UsedImplicitly]
        public static RequestRemove RemoveItemFromChest(this Container container, Container targetContainer, Vector2i from, Vector2i to, int dragAmount = 1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, targetContainer.GetInventory(), targetContainer.m_nview.m_zdo.m_uid, from, to, dragAmount, switchItem);
        }

        public static RequestRemove RemoveItemFromChest(this Container container, Player targetPlayer, Vector2i from, Vector2i to, int dragAmount = 1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, targetPlayer.GetInventory(), targetPlayer.GetZDOID(), from, to, dragAmount, switchItem);
        }

        public static RequestRemove RemoveItemFromChest(this Container container, Inventory targetInventory, ZDOID sender, Vector2i from, Vector2i to, int dragAmount = 1, ItemDrop.ItemData switchItem = null) {
            RequestRemove request = new RequestRemove(from, to, dragAmount, targetInventory.m_name, switchItem, sender);

            if (BypassSelfRouting && container != null && container.m_nview && container.m_nview.IsValid() && container.m_nview.IsOwner()) {
                RequestRemoveResponse response = container.GetInventory().RequestItemRemove(request);
                InventoryHandler.RPC_RequestItemRemoveResponse(targetInventory, response);
                return null;
            }

            InventoryHandler.BlockSlot(request.toPos);

            if (container != null && container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemRemove", request.WriteToPackage());
            }

            return request;
        }
    }
}
