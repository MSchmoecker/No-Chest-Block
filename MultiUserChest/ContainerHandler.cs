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
        public static RequestAdd AddItemToChest(this Container containerTo, ItemDrop.ItemData item, Container targetContainer, Vector2i to, int dragAmount = -1, bool allowSwitch = false) {
            return AddItemToChest(containerTo, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount, allowSwitch);
        }

        public static RequestAdd AddItemToChest(this Container container, ItemDrop.ItemData item, Inventory targetInventory, Vector2i to, ZDOID sender, int dragAmount = -1, bool allowSwitch = false) {
            dragAmount = PossibleDragAmount(container.GetInventory(), item, to, dragAmount);

            if (dragAmount <= 0) {
                return new RequestAdd(Vector2i.zero, 0, null, "", false, ZDOID.None);
            }

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

        [UsedImplicitly]
        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Container targetContainer, Vector2i to, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, item, targetContainer.GetInventory(), to, targetContainer.m_nview.m_zdo.m_uid, dragAmount, switchItem);
        }

        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Player targetPlayer, Vector2i to, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            return RemoveItemFromChest(container, item, targetPlayer.GetInventory(), to, targetPlayer.GetZDOID(), dragAmount, switchItem);
        }

        public static RequestRemove RemoveItemFromChest(this Container container, ItemDrop.ItemData item, Inventory targetInventory, Vector2i to, ZDOID sender, int dragAmount = -1, ItemDrop.ItemData switchItem = null) {
            dragAmount = PossibleDragAmount(targetInventory, item, to, dragAmount);

            if (dragAmount <= 0) {
                return new RequestRemove(Vector2i.zero, Vector2i.zero, 0, "", null, ZDOID.None);
            }

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
