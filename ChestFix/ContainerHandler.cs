using System;
using UnityEngine;

namespace ChestFix {
    public static class ContainerHandler {
        public static void RPC_RequestItemAdd(Container container, long l, ZPackage package) {
            ZPackage response = container.GetInventory().RequestItemAdd(l, package);
            container.m_nview.InvokeRPC(l, "RequestItemAddResponse", response);
        }

        public static void RPC_RequestItemRemove(Container container, long l, ZPackage package) {
            ZPackage response = container.GetInventory().RequestItemRemove(l, package);
            container.m_nview.InvokeRPC(l, "RequestItemRemoveResponse", response);
        }

        public static void RPC_RequestItemConsume(Container container, long l, ZPackage package) {
            ZPackage response = container.GetInventory().RequestItemConsume(l, package);
            container.m_nview.InvokeRPC(l, "RequestItemConsumeResponse", response);
        }

        public static void RPC_RequestItemMove(Container container, long sender, ZPackage package) {
            Log.LogInfo("RPC_RequestItemMove");

            RequestMove request = new RequestMove(package);

            Vector2i fromPos = request.fromPos;
            Vector2i toPos = request.toPos;
            int dragAmount = request.dragAmount;

            ItemDrop.ItemData from = container.GetInventory().GetItemAt(fromPos.x, fromPos.y);

            if (from == null) {
                Log.LogInfo("from == null: true");
                container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", false);
                return;
            }

            if (InventoryHelper.MoveItem(container.GetInventory(), from, dragAmount, toPos)) {
                ZDOMan.instance.ForceSendZDO(container.m_nview.GetZDO().m_uid);
                container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", true);
                return;
            }

            container.m_nview.InvokeRPC(sender, "RequestItemMoveResponse", false);
        }

        public static ZPackage RequestItemAdd(this Inventory inventory, long sender, ZPackage package) {
            Log.LogInfo("RPC_RequestItemAdd");

            RequestAdd request = new RequestAdd(package);
            request.PrintDebug();

            Vector2i fromInventory = request.fromInventory;
            Vector2i toContainer = request.toContainer;
            int dragAmount = request.dragAmount;
            ItemDrop.ItemData dragItem = request.dragItem;
            bool allowSwitch = request.allowSwitch;

            bool added = false;
            bool switched = false;
            bool dontAdd = false;
            int amount = Mathf.Min(dragAmount, dragItem.m_stack);

            ItemDrop.ItemData prevItem = inventory.GetItemAt(toContainer.x, toContainer.y);

            if (prevItem != null) {
                if (InventoryHelper.IsSameItem(prevItem, dragItem)) {
                    amount = Mathf.Min(prevItem.m_shared.m_maxStackSize - prevItem.m_stack, dragAmount);
                } else if (dragItem.m_stack != dragAmount) {
                    dontAdd = true;
                    amount = 0;
                } else {
                    if (allowSwitch) {
                        inventory.RemoveItem(prevItem, prevItem.m_stack);
                        switched = true;
                    } else {
                        dontAdd = true;
                        amount = 0;
                    }
                }
            }

            if (!dontAdd) {
                added = inventory.AddItem(dragItem, amount, toContainer.x, toContainer.y);
            }

            return new RequestAddResponse(added, fromInventory, amount, switched ? prevItem : null).WriteToPackage();
        }

        public static ZPackage RequestItemRemove(this Inventory inventory, long sender, ZPackage package) {
            Log.LogInfo("RPC_RequestItemRemove");

            RequestRemove request = new RequestRemove(package);
            request.PrintDebug();

            Vector2i fromContainer = request.fromContainer;
            Vector2i toInventory = request.toInventory;
            int dragAmount = request.dragAmount;
            ItemDrop.ItemData switchItem = request.switchItem;

            ItemDrop.ItemData from = inventory.GetItemAt(request.fromContainer.x, request.fromContainer.y);

            if (from == null) {
                Log.LogInfo("from is null");
                return new RequestRemoveResponse(false, 0, false, toInventory, null).WriteToPackage();
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
                    switched = inventory.AddItem(switchItem, switchItem.m_stack, fromContainer.x, fromContainer.y);
                }
            }

            ItemDrop.ItemData responseItem = from.Clone();
            responseItem.m_stack = removedAmount;
            return new RequestRemoveResponse(removed, removedAmount, switched, toInventory, responseItem).WriteToPackage();
        }

        private static ZPackage RequestItemConsume(this Inventory inventory, long sender, ZPackage package) {
            RequestConsume request = new RequestConsume(package);

            ItemDrop.ItemData toConsume = inventory.GetItemAt(request.itemPosX, request.itemPosY);

            if (toConsume == null || toConsume.m_stack <= 0) {
                return new RequestConsumeResponse(item: null).WriteToPackage();
            }

            inventory.RemoveOneItem(toConsume);
            return new RequestConsumeResponse(toConsume).WriteToPackage();;
        }
    }
}
