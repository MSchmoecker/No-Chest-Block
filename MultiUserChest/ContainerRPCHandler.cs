using System;
using System.Collections.Generic;
using MultiUserChest.Patches;
using UnityEngine;

namespace MultiUserChest {
    public static class ContainerRPCHandler {
        public static void RPC_RequestItemAdd(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, ContainerPatch.ItemAddResponseRPC, RequestItemAdd, () => new RequestChestAdd(package));
        }

        public static void RPC_RequestItemRemove(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, ContainerPatch.ItemRemoveResponseRPC, RequestItemRemove, () => new RequestChestRemove(package));
        }

        public static void RPC_RequestItemConsume(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, ContainerPatch.ItemConsumeResponseRPC, RequestItemConsume, () => new RequestConsume(package));
        }

        public static void RPC_RequestItemMove(Container container, long sender, ZPackage package) {
#if FULL_DEBUG
            Log.LogDebug(nameof(RPC_RequestItemMove));
#endif
            HandleRPC(container, sender, ContainerPatch.ItemMoveResponseRPC, RequestItemMove, () => new RequestMove(package));
        }

        public static void RPC_RequestTakeAllItems(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, ContainerPatch.ItemsTakeAllResponseRPC, RequestTakeAllItems, () => new RequestTakeAll(package));
        }

        public static void RPC_RequestDrop(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, ContainerPatch.ItemDropResponseRPC, RequestDrop, () => new RequestDrop(package));
        }

        private static void HandleRPC<TResponse, TInput>(Container container, long target, string rpcInvoke, Func<Inventory, TInput, TResponse> message, Func<TInput> input) where TResponse : new() where TInput : IPackage {
            if (!container.IsOwner()) {
                Log.LogDebug("I am not the owner");
                container.m_nview.InvokeRPC(target, rpcInvoke, Unpack(new TResponse()));
                return;
            }

            TInput inputPackage = input();
            inputPackage.PrintDebug();

            container.m_nview.InvokeRPC(target, rpcInvoke, Unpack(message(container.m_inventory, inputPackage)));
        }

        private static object Unpack(object input, bool debugPrint = true) {
            if (input is IPackage package) {
                if (debugPrint) {
                    package.PrintDebug();
                }

                return package.WriteToPackage();
            }

            return input;
        }

        public static RequestChestAddResponse RequestItemAdd(this Inventory inventory, RequestChestAdd request) {
            if (request.toPos.x < 0 || request.toPos.y < 0) {
                return AddToAnySlot(inventory, request);
            }

            return AddToSlot(inventory, request);
        }

        private static RequestChestAddResponse AddToSlot(Inventory inventory, RequestChestAdd request) {
            int dragAmount = request.dragAmount;
            ItemDrop.ItemData dragItem = request.dragItem;

            int amount = Mathf.Min(dragAmount, dragItem.m_stack);
            bool canStack = CanStack(inventory, request, ref amount, out ItemDrop.ItemData switched);

            if (!canStack) {
                dragItem.m_stack = dragAmount;
                return new RequestChestAddResponse(false, dragItem.m_gridPos, 0, request.fromInventoryHash, dragItem, request.sender);
            }

            bool added = inventory.AddItemToInventory(dragItem, amount, request.toPos);

            if (!added || amount != dragAmount) {
                switched = dragItem;
                switched.m_stack = dragAmount - amount;
            }

            if (switched != null) {
                switched.m_gridPos = request.dragItem.m_gridPos;
            }

            return new RequestChestAddResponse(added, request.dragItem.m_gridPos, amount, request.fromInventoryHash, switched, request.sender);
        }

        private static bool CanStack(Inventory inventory, RequestChestAdd request, ref int amount, out ItemDrop.ItemData removedItem) {
            ItemDrop.ItemData prevItem = inventory.GetItemAt(request.toPos.x, request.toPos.y);
            removedItem = null;

            if (prevItem == null) {
                return true;
            }

            if (InventoryHelper.IsSameItem(prevItem, request.dragItem)) {
                amount = Mathf.Min(prevItem.m_shared.m_maxStackSize - prevItem.m_stack, request.dragAmount);
                return true;
            }

            if (request.dragItem.m_stack != request.dragAmount) {
                return false;
            }

            if (request.allowSwitch) {
                inventory.RemoveItem(prevItem, prevItem.m_stack);
                removedItem = prevItem;
                return true;
            }

            return false;
        }

        private static RequestChestAddResponse AddToAnySlot(Inventory inventory, RequestChestAdd request) {
            Inventory tmp = new Inventory("tmp", null, 1, 1);
            tmp.AddItem(request.dragItem.Clone(), request.dragAmount, 0, 0);
            inventory.MoveItemToThis(tmp, tmp.GetItemAt(0, 0));

            ItemDrop.ItemData now = tmp.GetItemAt(0, 0);

            if (now == null) {
                return new RequestChestAddResponse(true, request.dragItem.m_gridPos, request.dragAmount, request.fromInventoryHash, null, request.sender);
            }

            ItemDrop.ItemData back = request.dragItem.Clone();
            int amount = request.dragItem.m_stack - now.m_stack;
            back.m_stack -= amount;

            bool success = now.m_stack != request.dragItem.m_stack;
            return new RequestChestAddResponse(success, request.dragItem.m_gridPos, amount, request.fromInventoryHash, back, request.sender);
        }

        public static RequestChestRemoveResponse RequestItemRemove(this Inventory inventory, RequestChestRemove request) {
            Vector2i fromContainer = request.fromPos;
            Vector2i toInventory = request.toPos;
            int dragAmount = request.dragAmount;
            ItemDrop.ItemData switchItem = request.switchItem;

            ItemDrop.ItemData from = inventory.GetItemAt(request.fromPos.x, request.fromPos.y);

            if (from == null) {
                Log.LogDebug("from is null");
                return new RequestChestRemoveResponse(false, 0, false, toInventory, request.fromInventoryHash, null, request.sender);
            }

            int removedAmount = 0;
            bool removed = false;
            bool switched = false;

            if (switchItem == null) {
                removedAmount = Mathf.Min(from.m_stack, dragAmount);
                removed = inventory.RemoveItem(from, removedAmount);
            } else {
                if (InventoryHelper.IsSameItem(from, switchItem)) {
                    removedAmount = Mathf.Min(switchItem.m_shared.m_maxStackSize - switchItem.m_stack, dragAmount);
                    removed = inventory.RemoveItem(from, removedAmount);
                } else if (dragAmount == from.m_stack) {
                    removed = inventory.RemoveItem(from, dragAmount);
                    removedAmount = Mathf.Min(from.m_stack, dragAmount);
                    switched = inventory.AddItemToInventory(switchItem, switchItem.m_stack, fromContainer);
                }
            }

            ItemDrop.ItemData responseItem = from.Clone();
            responseItem.m_stack = removedAmount;
            return new RequestChestRemoveResponse(removed, removedAmount, switched, toInventory, request.fromInventoryHash, responseItem, request.sender);
        }

        public static RequestConsumeResponse RequestItemConsume(this Inventory inventory, RequestConsume request) {
            ItemDrop.ItemData toConsume = inventory.GetItemAt(request.itemPosX, request.itemPosY);

            if (toConsume == null || toConsume.m_stack <= 0) {
                return new RequestConsumeResponse(null, false, 0);
            }

            inventory.RemoveOneItem(toConsume);
            ItemDrop.ItemData returnItem = toConsume.Clone();
            returnItem.m_stack = 1;
            return new RequestConsumeResponse(returnItem, true, 1);
        }

        private static bool RequestItemMove(this Inventory inventory, RequestMove request) {
            Vector2i fromPos = request.fromPos;
            Vector2i toPos = request.toPos;
            int dragAmount = request.dragAmount;

            ItemDrop.ItemData from = inventory.GetItemAt(fromPos.x, fromPos.y);

            if (from == null) {
                return false;
            }

            return InventoryHelper.MoveItem(inventory, from, dragAmount, toPos);
        }

        private static RequestTakeAll RequestTakeAllItems(this Inventory inventory, RequestTakeAll request) {
            List<ItemDrop.ItemData> moved = new List<ItemDrop.ItemData>();

            foreach (ItemDrop.ItemData item in request.items) {
                ItemDrop.ItemData existing = inventory.GetItemAt(item.m_gridPos.x, item.m_gridPos.y);

                if (existing != null) {
                    moved.Add(existing.Clone());
                    inventory.RemoveItem(existing, item.m_stack);
                }
            }

            return new RequestTakeAll(moved);
        }

        public static RequestDropResponse RequestDrop(this Inventory inventory, RequestDrop request) {
            ItemDrop.ItemData from = inventory.GetItemAt(request.targetContainerSlot.x, request.targetContainerSlot.y);

            int removedAmount = Mathf.Min(from.m_stack, request.amount);
            inventory.RemoveItem(from, removedAmount);

            ItemDrop.ItemData responseItem = from.Clone();
            responseItem.m_stack = removedAmount;

            return new RequestDropResponse(responseItem, request.sender, removedAmount != 0, removedAmount);
        }
    }
}
