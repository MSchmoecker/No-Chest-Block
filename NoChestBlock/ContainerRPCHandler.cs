using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoChestBlock {
    public static class ContainerRPCHandler {
        public static void RPC_RequestItemAdd(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, "RequestItemAddResponse", RequestItemAdd, () => new RequestAdd(package));
        }

        public static void RPC_RequestItemRemove(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, "RequestItemRemoveResponse", RequestItemRemove, () => new RequestRemove(package));
        }

        public static void RPC_RequestItemConsume(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, "RequestItemConsumeResponse", RequestItemConsume, () => new RequestConsume(package));
        }

        public static void RPC_RequestItemMove(Container container, long sender, ZPackage package) {
            Log.LogDebug("RequestItemMove");
            HandleRPC(container, sender, "RequestItemMoveResponse", RequestItemMove, () => new RequestMove(package));
        }

        public static void RPC_RequestTakeAllItems(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, "RequestTakeAllItemsResponse", RequestTakeAllItems, () => new RequestTakeAll(package));
        }

        public static void RPC_RequestDrop(Container container, long sender, ZPackage package) {
            HandleRPC(container, sender, "RequestDropResponse", RequestDrop, () => new RequestDrop(package));
        }

        private static void HandleRPC<T, T2>(Container container, long target, string rpcInvoke, Func<Inventory, T2, T> message, Func<T2> input) where T : new() where T2 : IPackage {
            if (!container.IsOwner()) {
                Log.LogDebug("I am not the owner");
                container.m_nview.InvokeRPC(target, rpcInvoke, Unpack(new T()));
                return;
            }

            T2 inputPackage = input();
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

        public static RequestAddResponse RequestItemAdd(this Inventory inventory, RequestAdd request) {
            if (request.toPos.x < 0 || request.toPos.y < 0) {
                return AddToAnySlot(inventory, request);
            }

            return AddToSlot(inventory, request);
        }

        private static RequestAddResponse AddToSlot(Inventory inventory, RequestAdd request) {
            Vector2i fromInventory = request.fromPos;
            int dragAmount = request.dragAmount;
            ItemDrop.ItemData dragItem = request.dragItem;

            int amount = Mathf.Min(dragAmount, dragItem.m_stack);
            bool canStack = CanStack(inventory, request, ref amount, out ItemDrop.ItemData switched);

            if (!canStack) {
                dragItem.m_stack = dragAmount;
                return new RequestAddResponse(false, fromInventory, 0, request.fromInventoryHash, dragItem, request.sender);
            }

            bool added = inventory.AddItemToInventory(dragItem, amount, request.toPos);

            if (!added || amount != dragAmount) {
                switched = dragItem;
                switched.m_stack = dragAmount - amount;
            }

            if (switched != null) {
                switched.m_gridPos = request.fromPos;
            }

            return new RequestAddResponse(added, fromInventory, amount, request.fromInventoryHash, switched, request.sender);
        }

        private static bool CanStack(Inventory inventory, RequestAdd request, ref int amount, out ItemDrop.ItemData removedItem) {
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

        private static RequestAddResponse AddToAnySlot(Inventory inventory, RequestAdd request) {
            Inventory tmp = new Inventory("tmp", null, 1, 1);
            tmp.AddItem(request.dragItem.Clone(), request.dragAmount, 0, 0);
            inventory.MoveItemToThis(tmp, tmp.GetItemAt(0, 0));

            ItemDrop.ItemData now = tmp.GetItemAt(0, 0);

            if (now == null) {
                return new RequestAddResponse(true, request.fromPos, request.dragAmount, request.fromInventoryHash, null, request.sender);
            }

            ItemDrop.ItemData back = request.dragItem.Clone();
            int amount = request.dragItem.m_stack - now.m_stack;
            back.m_stack -= amount;

            bool success = now.m_stack != request.dragItem.m_stack;
            return new RequestAddResponse(success, request.fromPos, amount, request.fromInventoryHash, back, request.sender);
        }

        public static RequestRemoveResponse RequestItemRemove(this Inventory inventory, RequestRemove request) {
            Vector2i fromContainer = request.fromPos;
            Vector2i toInventory = request.toPos;
            int dragAmount = request.dragAmount;
            ItemDrop.ItemData switchItem = request.switchItem;

            ItemDrop.ItemData from = inventory.GetItemAt(request.fromPos.x, request.fromPos.y);

            if (from == null) {
                Log.LogDebug("from is null");
                return new RequestRemoveResponse(false, 0, false, toInventory, request.fromInventoryHash, null, request.sender);
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
            return new RequestRemoveResponse(removed, removedAmount, switched, toInventory, request.fromInventoryHash, responseItem, request.sender);
        }

        private static RequestConsumeResponse RequestItemConsume(this Inventory inventory, RequestConsume request) {
            ItemDrop.ItemData toConsume = inventory.GetItemAt(request.itemPosX, request.itemPosY);

            if (toConsume == null || toConsume.m_stack <= 0) {
                return new RequestConsumeResponse(item: null);
            }

            inventory.RemoveOneItem(toConsume);
            return new RequestConsumeResponse(toConsume);
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

            return new RequestDropResponse(responseItem);
        }
    }
}
