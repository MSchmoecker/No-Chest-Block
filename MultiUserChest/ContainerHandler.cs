using System;
using System.Collections.Generic;
using MultiUserChest.Patches;
using UnityEngine;

namespace MultiUserChest {
    public static class ContainerHandler {
        [Obsolete]
        public static RequestAdd AddItemToChest(this Container containerTo, ItemDrop.ItemData item, Container targetContainer, Vector2i to, int dragAmount = -1, bool allowSwitch = false) {
            return AddItemToChest(containerTo, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount);
        }

        public static RequestChestAdd AddItemToChest(this Container container, ItemDrop.ItemData item, Inventory sourceInventory, Vector2i to, ZDOID sender, int dragAmount = -1) {
            if (!container || !container.m_nview || !container.m_nview.HasOwner()) {
                return new RequestChestAdd(Vector2i.zero, 0, null, null, null);
            }

            dragAmount = PossibleDragAmount(container.GetInventory(), item, to, dragAmount);
            ItemDrop.ItemData itemAtChest = container.GetInventory().GetItemAt(to.x, to.y);
            bool cannotStack = itemAtChest != null && dragAmount != item.m_stack && !InventoryHelper.IsSameItem(itemAtChest, item);

            if (dragAmount <= 0 || cannotStack || !sourceInventory.ContainsItem(item)) {
                return new RequestChestAdd(Vector2i.zero, 0, null, null, null);
            }

            RequestChestAdd request = new RequestChestAdd(to, dragAmount, item, sourceInventory, container.GetInventory());
            InventoryBlock.Get(sourceInventory).BlockSlot(item.m_gridPos);
            InventoryPreview.AddPackage(request);

            sourceInventory.RemoveItem(item, dragAmount);

            if (container.m_nview.IsOwner()) {
                RequestChestAddResponse response = container.GetInventory().RequestItemAdd(request);
                InventoryHandler.RPC_RequestItemAddResponse(sourceInventory, response);
                return null;
            }

#if DEBUG
            Timer.Start(request);
#endif
            container.m_nview.InvokeRPC(ContainerPatch.ItemAddRPC, request.WriteToPackage());
            return request;
        }

        internal static int PossibleDragAmount(Inventory inventoryTo, ItemDrop.ItemData dragItem, Vector2i to, int dragAmount) {
            if (dragAmount < 0) {
                dragAmount = dragItem.m_stack;
            } else {
                dragAmount = Mathf.Min(dragAmount, dragItem.m_stack);
            }

            if (to.x >= 0 && to.y >= 0) {
                ItemDrop.ItemData targetItem = inventoryTo.GetItemAt(to.x, to.y);

                if (targetItem != null && dragAmount < dragItem.m_stack) {
                    if (InventoryHelper.IsSameItem(targetItem, dragItem)) {
                        return Mathf.Min(dragAmount, targetItem.m_shared.m_maxStackSize - targetItem.m_stack);
                    }

                    return 0;
                }
            } else {
                bool hasEmptySlot = inventoryTo.HaveEmptySlot();

                if (!hasEmptySlot) {
                    int freeStackSpace = inventoryTo.FindFreeStackSpace(dragItem.m_shared.m_name);
                    return Mathf.Min(dragAmount, freeStackSpace);
                }
            }

            return dragAmount;
        }

        [Obsolete]
        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Container targetContainer, Vector2i to, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount, switchItem);
        }

        [Obsolete]
        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Player targetPlayer, Vector2i to, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, item, targetPlayer.GetInventory(), to, targetPlayer.GetZDOID(), dragAmount, switchItem);
        }

        public static RequestChestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Inventory destinationInventory, Vector2i to, ZDOID sender, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            if (!container || !container.m_nview || !container.m_nview.HasOwner()) {
                return new RequestChestRemove(Vector2i.zero, Vector2i.zero, 0, null, container.GetInventory(), destinationInventory);
            }

            dragAmount = PossibleDragAmount(destinationInventory, item, to, dragAmount);

            if (dragAmount <= 0 || InventoryBlock.Get(destinationInventory).IsSlotBlocked(to)) {
                return new RequestChestRemove(Vector2i.zero, Vector2i.zero, 0, null, container.GetInventory(), destinationInventory);
            }

            RequestChestRemove request = new RequestChestRemove(item.m_gridPos, to, dragAmount, switchItem, container.GetInventory(), destinationInventory);

            if (switchItem != null) {
                if (!InventoryHelper.IsSameItem(item, switchItem)) {
                    if (dragAmount != item.m_stack) {
                        return new RequestChestRemove(Vector2i.zero, Vector2i.zero, 0, null, container.GetInventory(), destinationInventory);
                    }

                    destinationInventory.RemoveItem(switchItem);
                }
            }

            InventoryBlock.Get(destinationInventory).BlockSlot(request.toPos);
            InventoryPreview.AddPackage(request);

            if (container.m_nview.IsOwner()) {
                RequestChestRemoveResponse response = container.GetInventory().RequestItemRemove(request);
                InventoryHandler.RPC_RequestItemRemoveResponse(destinationInventory, response);
                return null;
            }

#if DEBUG
            Timer.Start(request);
#endif

            container.m_nview.InvokeRPC(ContainerPatch.ItemRemoveRPC, request.WriteToPackage());
            return request;
        }

        public static void MoveItemInChest(this Container container, ItemDrop.ItemData item, Vector2i toPos, int dragAmount) {
            RequestMove request = new RequestMove(item, toPos, dragAmount);
#if DEBUG
            Timer.Start(request);
#endif
            container.m_nview.InvokeRPC(ContainerPatch.ItemMoveRPC, request.WriteToPackage());
        }
    }
}
