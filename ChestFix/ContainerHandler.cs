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

        public static void RPC_RequestItemMove(Container container, long sender, ZPackage package) {
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

        public static ZPackage RequestItemAdd(this Inventory inventory, long sender, ZPackage package) {
            ZPackage response = new ZPackage();

            Log.LogInfo("RPC_RequestItemAdd");

            Vector2i fromInventory = package.ReadVector2i();
            Vector2i toContainer = package.ReadVector2i();
            int dragAmount = package.ReadInt();
            ItemDrop.ItemData dragItem = InventoryHelper.LoadItemFromPackage(package, toContainer);
            bool allowSwitch = package.ReadBool();

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

            response.Write(fromInventory);
            response.Write(added);
            response.Write(amount);
            response.Write(switched);

            if (switched) {
                InventoryHelper.WriteItemToPackage(prevItem, response);
            }

            return response;
        }

        public static ZPackage RequestItemRemove(this Inventory inventory, long sender, ZPackage package) {
            ZPackage response = new ZPackage();

            Log.LogInfo("RPC_RequestItemRemove");

            // read package data
            Vector2i fromContainer = package.ReadVector2i();
            Vector2i toInventory = package.ReadVector2i();
            int dragAmount = package.ReadInt();
            bool hasSwitchItem = package.ReadBool();

            ItemDrop.ItemData dragItem = hasSwitchItem ? InventoryHelper.LoadItemFromPackage(package, fromContainer) : null;
            ItemDrop.ItemData from = inventory.GetItemAt(fromContainer.x, fromContainer.y);

            if (from == null) {
                Log.LogInfo("from is null");
                response.Write(false); // not removed
                response.Write(0);
                response.Write(false); // not switched
                response.Write(toInventory);
                response.Write(false); // no response item
                return response;
            }

            int removedAmount = 0;
            bool removed = false;
            bool switched = false;

            if (dragItem == null) {
                removedAmount = Mathf.Min(from.m_stack, dragAmount);
                removed = inventory.RemoveItem(from, removedAmount);
            } else {
                if (InventoryHelper.IsSameItem(from, dragItem)) {
                    removedAmount = Mathf.Min(dragItem.m_shared.m_maxStackSize - dragItem.m_stack, dragAmount);
                    removed = inventory.RemoveItem(from, removedAmount);
                } else if (dragAmount == from.m_stack) {
                    removed = inventory.RemoveItem(from, dragAmount);
                    removedAmount = Mathf.Min(from.m_stack, dragAmount);
                    switched = inventory.AddItem(dragItem, dragItem.m_stack, fromContainer.x, fromContainer.y);
                }
            }

            response.Write(removed);
            response.Write(removedAmount);
            response.Write(switched);
            response.Write(toInventory);
            response.Write(true); // has response item

            // if (removed) {
            InventoryHelper.WriteItemToPackage(from, response);
            // }

            return response;
        }
    }
}
