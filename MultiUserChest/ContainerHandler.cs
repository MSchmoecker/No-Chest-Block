using System.Collections.Generic;
using JetBrains.Annotations;
using MultiUserChest.Patches;
using UnityEngine;

namespace MultiUserChest {
    public static class ContainerHandler {
        public static void TakeAll(Container container, Inventory playerInventory) {
            List<ItemDrop.ItemData> wanted = InventoryHelper.GetAllMoveableItems(container.GetInventory(), playerInventory);

            InventoryBlock.Get(playerInventory).BlockAllSlots(true);

            RequestTakeAll request = new RequestTakeAll(wanted);
            Timer.Start(request);

            if (container != null && container.m_nview) {
                container.m_nview.InvokeRPC(ContainerPatch.ItemsTakeAllRPC, request.WriteToPackage());
            }
        }

        [UsedImplicitly]
        public static RequestAdd AddItemToChest(this Container containerTo, ItemDrop.ItemData item, Container targetContainer, Vector2i to,
            int dragAmount = -1, bool allowSwitch = false) {
            return AddItemToChest(containerTo, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount,
                allowSwitch);
        }

        public static RequestAdd AddItemToChest(this Container container, ItemDrop.ItemData item, Inventory targetInventory, Vector2i to,
            ZDOID sender, int dragAmount = -1, bool allowSwitch = false) {
            dragAmount = dragAmount < 0 ? item.m_stack : Mathf.Min(dragAmount, item.m_stack);
            RequestAdd request = new RequestAdd(to, dragAmount, item, targetInventory.m_name, allowSwitch, sender);
            InventoryBlock.Get(targetInventory).BlockSlot(item.m_gridPos);

            targetInventory.RemoveItem(item, dragAmount);

            if (container != null && container.m_nview) {
                if (container.m_nview.IsOwner()) {
                    RequestAddResponse response = container.GetInventory().RequestItemAdd(request);
                    InventoryHandler.RPC_RequestItemAddResponse(targetInventory, response);
                    return null;
                }

                Timer.Start(request);
                container.m_nview.InvokeRPC(ContainerPatch.ItemAddRPC, request.WriteToPackage());
            }

            return request;
        }

        [UsedImplicitly]
        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Container targetContainer,
            Vector2i to, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount,
                switchItem);
        }

        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Player targetPlayer, Vector2i to,
            int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, item, targetPlayer.GetInventory(), to, targetPlayer.GetZDOID(), dragAmount, switchItem);
        }

        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Inventory targetInventory,
            Vector2i to, ZDOID sender, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            dragAmount = dragAmount < 0 ? item.m_stack : dragAmount;
            RequestRemove request = new RequestRemove(item.m_gridPos, to, dragAmount, targetInventory.m_name, switchItem, sender);
            InventoryBlock.Get(targetInventory).BlockSlot(request.toPos);

            if (switchItem != null) {
                if (!InventoryHelper.IsSameItem(item, switchItem)) {
                    if (dragAmount != item.m_stack) {
                        return new RequestRemove(Vector2i.zero, Vector2i.zero, 0, "", null, ZDOID.None);
                    }

                    targetInventory.RemoveItem(switchItem);
                }
            }

            if (container != null && container.m_nview) {
                if (container.m_nview.IsOwner()) {
                    RequestRemoveResponse response = container.GetInventory().RequestItemRemove(request);
                    InventoryHandler.RPC_RequestItemRemoveResponse(targetInventory, response);
                    return null;
                }

                Timer.Start(request);
                container.m_nview.InvokeRPC(ContainerPatch.ItemRemoveRPC, request.WriteToPackage());
            }

            return request;
        }

        public static void MoveItemInChest(this Container container, Vector2i fromPos, Vector2i toPos, int dragAmount) {
            RequestMove request = new RequestMove(fromPos, toPos, dragAmount);
            Timer.Start(request);

            container.m_nview.InvokeRPC(ContainerPatch.ItemMoveRPC, request.WriteToPackage());
        }
    }
}
