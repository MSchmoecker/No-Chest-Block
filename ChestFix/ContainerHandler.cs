using System;
using UnityEngine;

namespace ChestFix {
    public static class ContainerHandler {
        public static void RPC_RequestItemMove(this Container container, long sender, ZPackage package) {
            Log.LogInfo("RPC_RequestItemMove");

            Vector2i fromPos = package.ReadVector2i();
            Vector2i toPos = package.ReadVector2i();
            int dragAmount = package.ReadInt();

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

        public static ZPackage RPC_RequestItemAdd(this Inventory inventory, long sender, ZPackage package) {
            ZPackage response = new ZPackage();

            Log.LogInfo("RPC_RequestItemAdd");

            Vector2i fromInventory = package.ReadVector2i();
            Vector2i toContainer = package.ReadVector2i();
            int dragAmount = package.ReadInt();
            ItemDrop.ItemData dragItem = InventoryHelper.LoadItemFromPackage(package, toContainer);

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
                    inventory.RemoveItem(prevItem, prevItem.m_stack);
                    switched = true;
                }
            }

            if (!dontAdd) {
                added = inventory.AddItem(dragItem, amount, toContainer.x, toContainer.y);
            }

            response.Write(added);
            response.Write(amount);
            response.Write(switched);

            if (switched) {
                InventoryHelper.WriteItemToPackage(prevItem, response);
            }

            return response;
        }

        public static ZPackage RPC_RequestItemRemove(this Inventory inventory, long sender, ZPackage package) {
            ZPackage response = new ZPackage();

            Log.LogInfo("RPC_RequestItemRemove");

            // read package data
            Vector2i fromContainer = package.ReadVector2i();
            Vector2i toInventory = package.ReadVector2i();
            int dragAmount = package.ReadInt();
            bool hasSwitchItem = package.ReadBool();

            ItemDrop.ItemData from = inventory.GetItemAt(fromContainer.x, fromContainer.y);

            if (from == null) {
                Log.LogInfo("from is null");
                response.Write(false); // not removed
                response.Write(0);
                response.Write(false); // not switched
                return response;
            }

            int removedAmount = Mathf.Min(from.m_stack, dragAmount);
            bool removed = false;

            if (!hasSwitchItem || from.m_stack - removedAmount == 0) {
                removed = inventory.RemoveItem(from, dragAmount);
            } else {
                removedAmount = 0;
            }

            bool switched = false;

            if (hasSwitchItem && removed) {
                switched = InventoryHelper.LoadItemIntoInventory(package, inventory, fromContainer, -1, -1);
                ItemDrop.ItemData addedItem = inventory.GetItemAt(fromContainer.x, fromContainer.y);

                if (InventoryHelper.IsSameItem(from, addedItem)) {
                    switched = false;
                    int stackSize = from.m_shared.m_maxStackSize;
                    int beforeAmount = removedAmount;
                    removedAmount = Mathf.Min(removedAmount, stackSize - addedItem.m_stack);

                    // set stack size by RemoveItem() to new value, value = beforeAmount - removedAmount
                    inventory.RemoveItem(addedItem, addedItem.m_stack - beforeAmount + removedAmount);
                }
            }

            response.Write(removed);
            response.Write(removedAmount);
            response.Write(switched);

            return response;
        }
    }
}
