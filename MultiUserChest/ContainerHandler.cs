using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MultiUserChest.Patches;
using UnityEngine;

namespace MultiUserChest {
    public static class ContainerHandler {
        public static void TakeAll(Container container, Inventory playerInventory) {
            List<ItemDrop.ItemData> wanted = InventoryHelper.GetAllMoveableItems(container.GetInventory(), playerInventory);
            InventoryBlock.Get(playerInventory).BlockAllSlots = true;
            RequestTakeAll request = new RequestTakeAll(wanted);
#if DEBUG
            Timer.Start(request);
#endif
            if (container != null && container.m_nview) {
                container.m_nview.InvokeRPC(ContainerPatch.ItemsTakeAllRPC, request.WriteToPackage());
            }
        }

        [Obsolete]
        public static RequestAdd AddItemToChest(this Container containerTo, ItemDrop.ItemData item, Container targetContainer, Vector2i to, int dragAmount = -1, bool allowSwitch = false) {
            return AddItemToChest(containerTo, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount);
        }

        public static RequestChestAdd AddItemToChest(this Container container, ItemDrop.ItemData item, Inventory targetInventory, Vector2i to, ZDOID sender, int dragAmount = -1) {
            dragAmount = PossibleDragAmount(container.GetInventory(), item, to, dragAmount);
            ItemDrop.ItemData itemAtChest = container.GetInventory().GetItemAt(to.x, to.y);
            bool cannotStack = itemAtChest != null && dragAmount != item.m_stack && !InventoryHelper.IsSameItem(itemAtChest, item);

            if (dragAmount <= 0 || cannotStack) {
                return new RequestChestAdd(Vector2i.zero, 0, null, "", ZDOID.None);
            }

            RequestChestAdd request = new RequestChestAdd(to, dragAmount, item, targetInventory.m_name, sender);
            InventoryBlock.Get(targetInventory).BlockSlot(item.m_gridPos);

            targetInventory.RemoveItem(item, dragAmount);

            if (container != null && container.m_nview) {
                if (container.m_nview.IsOwner()) {
                    RequestChestAddResponse response = container.GetInventory().RequestItemAdd(request);
                    InventoryHandler.RPC_RequestItemAddResponse(targetInventory, response);
                    return null;
                }

#if DEBUG
                Timer.Start(request);
#endif
                container.m_nview.InvokeRPC(ContainerPatch.ItemAddRPC, request.WriteToPackage());
            }

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

                if (targetItem != null) {
                    return Mathf.Min(dragAmount, targetItem.m_shared.m_maxStackSize - targetItem.m_stack);
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

        public static RequestChestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Inventory targetInventory, Vector2i to, ZDOID sender, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            dragAmount = PossibleDragAmount(targetInventory, item, to, dragAmount);

            if (dragAmount <= 0 || InventoryBlock.Get(targetInventory).IsSlotBlocked(to)) {
                return new RequestChestRemove(Vector2i.zero, Vector2i.zero, 0, "", null, ZDOID.None);
            }

            RequestChestRemove request = new RequestChestRemove(item.m_gridPos, to, dragAmount, targetInventory.m_name, switchItem, sender);

            if (switchItem != null) {
                if (!InventoryHelper.IsSameItem(item, switchItem)) {
                    if (dragAmount != item.m_stack) {
                        return new RequestChestRemove(Vector2i.zero, Vector2i.zero, 0, "", null, ZDOID.None);
                    }

                    targetInventory.RemoveItem(switchItem);
                }
            }

            InventoryBlock.Get(targetInventory).BlockSlot(request.toPos);

            if (container != null && container.m_nview) {
                if (container.m_nview.IsOwner()) {
                    RequestChestRemoveResponse response = container.GetInventory().RequestItemRemove(request);
                    InventoryHandler.RPC_RequestItemRemoveResponse(targetInventory, response);
                    return null;
                }

#if DEBUG
                Timer.Start(request);
#endif
                container.m_nview.InvokeRPC(ContainerPatch.ItemRemoveRPC, request.WriteToPackage());
            }

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
