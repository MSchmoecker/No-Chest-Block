using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;

namespace MultiUserChest.Patches {
    [HarmonyPatch]
    public static class InventoryPatch {
        public static readonly ConditionalWeakTable<ItemDrop.ItemData, Inventory> InventoryOfItem = new ConditionalWeakTable<ItemDrop.ItemData, Inventory>();
        private static readonly WeakReference<ItemDrop.ItemData> LastRemovedItem = new WeakReference<ItemDrop.ItemData>(null);
        private static bool allowItemSwap = true;

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
        [HarmonyPrefix, HarmonyPriority(Priority.VeryLow)]
        public static void AddItem1Prefix(Inventory __instance, ref bool __runOriginal, ItemDrop.ItemData item, int amount, int x, int y, ref bool __result) {
            if (!__runOriginal) {
                return;
            }

            // intercept movement if the item was added from/to an inventory not owned by the local instance
            bool intercepted = InterceptAddItem(__instance, item, amount, new Vector2i(x, y), out bool successfulAdded);

            if (intercepted) {
                __result = successfulAdded;
                __runOriginal = false;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData))]
        [HarmonyPrefix, HarmonyPriority(Priority.VeryLow)]
        public static void AddItem2Prefix(Inventory __instance, ref bool __runOriginal, ItemDrop.ItemData item, ref bool __result) {
            if (!__runOriginal) {
                return;
            }

            // intercept movement if the item was added from/to an inventory not owned by the local instance
            bool intercepted = InterceptAddItem(__instance, item, item.m_stack, new Vector2i(-1, -1), out bool successfulAdded);

            if (intercepted) {
                __result = successfulAdded;
                __runOriginal = false;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(ItemDrop.ItemData))]
        [HarmonyPrefix, HarmonyPriority(Priority.VeryLow)]
        public static void RemoveItemPrefix(Inventory __instance, ref bool __runOriginal, ref bool __result) {
            if (!__runOriginal) {
                return;
            }

            InventoryOwner owner = InventoryOwner.GetOwner(__instance);

            // forbid removing items from inventories not owned by the local instance
            if (owner != null && owner.IsValid() && !owner.ZNetView.IsOwner()) {
                __result = false;
                __runOriginal = false;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(ItemDrop.ItemData))]
        [HarmonyPostfix]
        public static void RemoveItemPostfix(Inventory __instance, ItemDrop.ItemData item, ref bool __result) {
            if (__result) {
                LastRemovedItem.SetTarget(item);
            } else {
                LastRemovedItem.SetTarget(null);
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll)), HarmonyPrefix, HarmonyPriority(Priority.First)]
        public static void MoveAllPrefix() {
            allowItemSwap = false;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void MoveAllPostfix() {
            allowItemSwap = true;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.Changed)), HarmonyPostfix]
        public static void AddItemPostfix(Inventory __instance) {
            foreach (Inventory inventory in __instance.GetInventories()) {
                AssignItemsOfInventory(inventory);
            }

            if (LastRemovedItem.TryGetTarget(out ItemDrop.ItemData lastItem) && InventoryOwner.GetOwner(lastItem)?.Inventory == __instance) {
                LastRemovedItem.SetTarget(null);
            }
        }

        private static void AssignItemsOfInventory(Inventory inventory) {
            foreach (ItemDrop.ItemData item in inventory.GetAllItems()) {
                InventoryOfItem.Remove(item);
                InventoryOfItem.Add(item, inventory);
            }
        }

        internal static bool InterceptAddItem(Inventory inventory, ItemDrop.ItemData item, int amount, Vector2i pos, out bool successfulAdded) {
            InventoryOwner from = InventoryOwner.GetOwner(item);
            InventoryOwner to = InventoryOwner.GetOwner(inventory);

#if DEBUG
            Log.LogDebug($"Try InterceptAddItem: {item.m_shared.m_name}; from: {from?.GetDescription() ?? "null"}, to: {to?.GetDescription() ?? "null"}");
#endif

            if (from == null || !from.IsValid() || to == null || !to.IsValid()) {
                successfulAdded = false;
                return false;
            }

            if (OdinShip.IsOdinShipInstalled() && (!from.ZNetView.IsOwner() || !to.ZNetView.IsOwner())) {
                bool toIsOdinShipContainer = to is ContainerInventoryOwner toContainer && toContainer.Container.IsOdinShipContainer();
                bool fromIsOdinShipContainer = from is ContainerInventoryOwner fromContainer && fromContainer.Container.IsOdinShipContainer();

                if (toIsOdinShipContainer || fromIsOdinShipContainer) {
                    successfulAdded = false;
                    return true;
                }
            }

            if (from.ZNetView.IsOwner() && !to.ZNetView.IsOwner()) {
                if (to is ContainerInventoryOwner toContainer) {
                    RequestChestAdd requestChestAdd = toContainer.Container.AddItemToChest(item, from.Inventory, pos, from.ZNetView.GetZDO().m_uid, amount);
                    successfulAdded = requestChestAdd?.dragItem?.m_stack == amount;
                    return true;
                }
            }

            if (!from.ZNetView.IsOwner() && to.ZNetView.IsOwner()) {
                if (from is ContainerInventoryOwner fromContainer) {
                    ItemDrop.ItemData switchItem = to.Inventory.GetItemAt(pos.x, pos.y);

                    if (switchItem == null && LastRemovedItem.TryGetTarget(out ItemDrop.ItemData lastItem) && InventoryOwner.GetOwner(lastItem) == to && lastItem.m_gridPos == pos) {
                        switchItem = lastItem;
                    }

                    if (!allowItemSwap && switchItem != null) {
                        successfulAdded = false;
                        return true;
                    }

                    if (switchItem != null && InventoryHelper.IsSameItem(item, switchItem)) {
                        switchItem = null;
                    }

                    RequestChestRemove requestChestRemove = fromContainer.Container.RemoveItemFromChest(item, to.Inventory, pos, to.ZNetView.GetZDO().m_uid, amount, switchItem);
                    successfulAdded = requestChestRemove?.dragAmount == amount;
                    return true;
                }
            }

            if (!from.ZNetView.IsOwner() && !to.ZNetView.IsOwner()) {
                if (from is ContainerInventoryOwner fromContainer && to is ContainerInventoryOwner toContainer && fromContainer.Container == toContainer.Container) {
                    fromContainer.Container.MoveItemInChest(item, pos, amount);
                    successfulAdded = true;
                    return true;
                }
            }

            successfulAdded = false;
            return false;
        }
    }
}
