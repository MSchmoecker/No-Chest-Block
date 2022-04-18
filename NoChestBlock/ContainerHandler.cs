using System.Collections.Generic;
using JetBrains.Annotations;

namespace NoChestBlock {
    public static class ContainerHandler {
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
        public static RequestAdd AddItemToChest(this Container containerTo, ItemDrop.ItemData item, Container targetContainer, Vector2i to, int dragAmount = -1, bool allowSwitch = false) {
            return AddItemToChest(containerTo, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount, allowSwitch);
        }

        public static RequestAdd AddItemToChest(this Container container, ItemDrop.ItemData item, Inventory targetInventory, Vector2i to, ZDOID sender, int dragAmount = -1, bool allowSwitch = false) {
            dragAmount = dragAmount < 0 ? item.m_stack : dragAmount;
            RequestAdd request = new RequestAdd(to, dragAmount, item, targetInventory.m_name, allowSwitch, sender);
            InventoryHandler.BlockSlot(item.m_gridPos);

            targetInventory.RemoveItem(item.m_shared.m_name, dragAmount);

            if (container != null && container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemAdd", request.WriteToPackage());
            }

            return request;
        }

        [UsedImplicitly]
        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Container targetContainer, Vector2i to, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount, switchItem);
        }

        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Player targetPlayer, Vector2i to, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, item, targetPlayer.GetInventory(), to, targetPlayer.GetZDOID(), dragAmount, switchItem);
        }

        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Inventory targetInventory, Vector2i to, ZDOID sender, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            dragAmount = dragAmount < 0 ? item.m_stack : dragAmount;
            RequestRemove request = new RequestRemove(item.m_gridPos, to, dragAmount, targetInventory.m_name, switchItem, sender);
            InventoryHandler.BlockSlot(request.toPos);

            if (container != null && container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemRemove", request.WriteToPackage());
            }

            return request;
        }
    }
}
