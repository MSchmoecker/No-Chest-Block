using System;
using System.Collections.Generic;
using MultiUserChest.Patches;
using UnityEngine;

namespace MultiUserChest {
    public static class ContainerRPCHandler {
        public static void RPC_RequestItemAdd(long sender, ZDOID containerId, ZPackage package) {
            HandleRPC(containerId, sender, ContainerPatch.ItemAddResponseRPC, RequestItemAdd, new RequestChestAdd(package));
        }

        public static void RPC_RequestItemRemove( long sender, ZDOID containerId,ZPackage package) {
            HandleRPC(containerId, sender, ContainerPatch.ItemRemoveResponseRPC, RequestItemRemove, new RequestChestRemove(package));
        }

        public static void RPC_RequestItemConsume(long sender, ZDOID containerId, ZPackage package) {
            HandleRPC(containerId, sender, ContainerPatch.ItemConsumeResponseRPC, RequestItemConsume, new RequestConsume(package));
        }

        public static void RPC_RequestItemMove(long sender, ZDOID containerId, ZPackage package) {
            HandleRPC(containerId, sender, ContainerPatch.ItemMoveResponseRPC, RequestItemMove, new RequestMove(package));
        }

        public static void RPC_RequestDrop(long sender, ZDOID containerId, ZPackage package) {
            HandleRPC(containerId, sender, ContainerPatch.ItemDropResponseRPC, RequestDrop, new RequestDrop(package));
        }

        private static void HandleRPC<TResponse, TInput>(ZDOID containerId, long target, string rpcInvoke, Func<Inventory, TInput, TResponse> message, TInput input) where TResponse : IResponse, new() where TInput : IPackage {
            ZDO zdo = ZDOMan.instance.GetZDO(containerId);
            ZNetView instance = zdo != null ? ZNetScene.instance.FindInstance(zdo) : null;

            TResponse response;

            if (!instance || !instance.IsOwner()) {
#if DEBUG
                if (!instance) {
                    Log.LogWarning($"{input.GetType().Name}: not handling RPC, ZNetView instance is null");
                } else if (!instance.IsOwner()) {
                    Log.LogWarning($"{input.GetType().Name}: not handling RPC, ZNetView instance is not owner");
                }
#endif

                response = FallbackResponse<TResponse>(input as IRequest);
            } else {
                Container container = instance.GetComponentInChildren<Container>();

                if (container) {
                    response = message(container.GetInventory(), input);
                } else {
                    Log.LogWarning($"{input.GetType().Name}: not handling RPC, Container component not found on {instance.name}");
                    response = FallbackResponse<TResponse>(input as IRequest);
                }
            }

#if DEBUG
            input.PrintDebug();
            response.PrintDebug();
#endif

            ZRoutedRpc.instance.InvokeRoutedRPC(target, rpcInvoke, containerId, response.WriteToPackage());
        }

        private static TResponse FallbackResponse<TResponse>(IRequest request) where TResponse : IResponse, new() {
            TResponse response = new TResponse() {
                SourceID = request?.RequestID ?? 0
            };

            if (request is RequestChestAdd addRequest && response is RequestChestAddResponse addResponse) {
                addResponse.switchItem = addRequest.dragItem;
                addResponse.inventoryPos = addRequest.dragItem?.m_gridPos ?? Vector2i.zero;
            } else if (request is RequestChestRemove removeRequest && response is RequestChestRemoveResponse removeResponse) {
                removeResponse.responseItem = removeRequest.switchItem;
            }

            return response;
        }

        public static RequestChestAddResponse RequestItemAdd(this Inventory inventory, RequestChestAdd request) {
            if (request.dragItem == null) {
                return new RequestChestAddResponse(request.RequestID, false, Vector2i.zero, 0, request.dragItem);
            }

            if (request.toPos.x < 0 || request.toPos.y < 0) {
                return AddToAnySlot(inventory, request);
            }

            return AddToSlot(inventory, request);
        }

        private static RequestChestAddResponse AddToSlot(Inventory inventory, RequestChestAdd request) {
            bool canStack = CanStack(inventory, request, out int stackSpace, out ItemDrop.ItemData switched);

            if (!canStack) {
                return new RequestChestAddResponse(request.RequestID, false, request.dragItem.m_gridPos, 0, request.dragItem);
            }

            bool added = inventory.AddItemToInventory(request.dragItem, stackSpace, request.toPos);

            if (!added) {
                stackSpace = 0;
            }

            if (!added || stackSpace != request.dragItem.m_stack) {
                switched = request.dragItem;
                switched.m_stack = request.dragItem.m_stack - stackSpace;
            }

            if (switched != null) {
                switched.m_gridPos = request.dragItem.m_gridPos;
            }

            return new RequestChestAddResponse(request.RequestID, added, request.dragItem.m_gridPos, stackSpace, switched);
        }

        private static bool CanStack(Inventory inventory, RequestChestAdd request, out int stackSpace, out ItemDrop.ItemData removedItem) {
            ItemDrop.ItemData prevItem = inventory.GetItemAt(request.toPos.x, request.toPos.y);
            removedItem = null;

            if (prevItem == null) {
                stackSpace = request.dragItem.m_stack;
                return true;
            }

            if (InventoryHelper.CanStack(prevItem, request.dragItem)) {
                stackSpace = Mathf.Min(prevItem.m_shared.m_maxStackSize - prevItem.m_stack, request.dragItem.m_stack);
                return true;
            }

            if (request.allowSwitch) {
                inventory.RemoveItem(prevItem, prevItem.m_stack);
                removedItem = prevItem;
                stackSpace = request.dragItem.m_stack;
                return true;
            }

            stackSpace = 0;
            return false;
        }

        private static RequestChestAddResponse AddToAnySlot(Inventory inventory, RequestChestAdd request) {
            Inventory tmp = new Inventory("tmp", null, 1, 1);
            tmp.AddItem(request.dragItem.Clone(), request.dragItem.m_stack, 0, 0);
            inventory.MoveItemToThis(tmp, tmp.GetItemAt(0, 0));

            ItemDrop.ItemData now = tmp.GetItemAt(0, 0);

            if (now == null) {
                return new RequestChestAddResponse(request.RequestID, true, request.dragItem.m_gridPos, request.dragItem.m_stack, null);
            }

            ItemDrop.ItemData back = request.dragItem.Clone();
            int amount = request.dragItem.m_stack - now.m_stack;
            back.m_stack -= amount;

            bool success = now.m_stack != request.dragItem.m_stack;
            return new RequestChestAddResponse(request.RequestID, success, request.dragItem.m_gridPos, amount, back);
        }

        public static RequestChestRemoveResponse RequestItemRemove(this Inventory inventory, RequestChestRemove request) {
            Vector2i fromContainer = request.fromPos;
            int dragAmount = request.dragAmount;
            ItemDrop.ItemData switchItem = request.switchItem;

            ItemDrop.ItemData from = inventory.GetItemAt(request.fromPos.x, request.fromPos.y);

            if (from == null) {
                Log.LogDebug("from is null");
                return new RequestChestRemoveResponse(request.RequestID, false, 0, false, null);
            }

            int removedAmount = 0;
            bool removed = false;
            bool switched = false;

            if (switchItem == null) {
                removedAmount = Mathf.Min(from.m_stack, dragAmount);
                removed = inventory.RemoveItem(from, removedAmount);
            } else {
                if (InventoryHelper.CanStack(from, switchItem)) {
                    removedAmount = Mathf.Min(switchItem.m_shared.m_maxStackSize - switchItem.m_stack, dragAmount);
                    removed = inventory.RemoveItem(from, removedAmount);
                } else if (dragAmount >= from.m_stack) {
                    removed = inventory.RemoveItem(from, dragAmount);
                    removedAmount = Mathf.Min(from.m_stack, dragAmount);
                    switched = inventory.AddItemToInventory(switchItem, switchItem.m_stack, fromContainer);

                    if (!switched) {
                        Log.LogWarning("Could not switch item in Remove Request");
                        InventoryHandler.DropItem(switchItem, switchItem.m_stack);
                    }
                } else {
                    return new RequestChestRemoveResponse(request.RequestID, false, 0, false, request.switchItem);
                }
            }

            ItemDrop.ItemData responseItem = from.Clone();
            responseItem.m_stack = removedAmount;
            return new RequestChestRemoveResponse(request.RequestID, removed, removedAmount, switched, responseItem);
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

        public static RequestMoveResponse RequestItemMove(this Inventory inventory, RequestMove request) {
            Vector2i fromPos = request.fromPos;
            Vector2i toPos = request.toPos;
            int dragAmount = request.dragAmount;

            ItemDrop.ItemData from = inventory.GetItemAt(fromPos.x, fromPos.y);

            if (from == null) {
                return new RequestMoveResponse(request.RequestID, false, 0);
            }

            if (from.PrefabName().GetStableHashCode() != request.itemHash) {
                return new RequestMoveResponse(request.RequestID, false, 0);
            }

            bool moved = InventoryHelper.MoveItem(inventory, from, dragAmount, toPos, out int movedAmount);
            return new RequestMoveResponse(request.RequestID, moved, movedAmount);
        }

        public static RequestDropResponse RequestDrop(this Inventory inventory, RequestDrop request) {
            ItemDrop.ItemData from = inventory.GetItemAt(request.targetContainerSlot.x, request.targetContainerSlot.y);

            if (from == null) {
                return new RequestDropResponse(null, request.sender, false, 0);
            }

            int removedAmount = Mathf.Min(from.m_stack, request.amount);
            inventory.RemoveItem(from, removedAmount);

            if (removedAmount == 0) {
                return new RequestDropResponse(null, request.sender, false, 0);
            }

            ItemDrop.ItemData responseItem = from.Clone();
            responseItem.m_stack = removedAmount;

            return new RequestDropResponse(responseItem, request.sender, true, removedAmount);
        }
    }
}
