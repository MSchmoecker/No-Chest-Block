﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;

namespace MultiUserChest.Patches {
    [HarmonyPatch]
    public static class InventoryPatch {
        public static readonly ConditionalWeakTable<ItemDrop.ItemData, Inventory> InventoryOfItem = new ConditionalWeakTable<ItemDrop.ItemData, Inventory>();

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
        [HarmonyPrefix]
        public static bool AddItem1Prefix(Inventory __instance, ItemDrop.ItemData item, int amount, int x, int y, ref bool __result) {
            bool intercepted = InterceptAddItem(__instance, item, amount, new Vector2i(x, y), out bool successfulAdded);

            if (intercepted) {
                __result = successfulAdded;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData))]
        [HarmonyPrefix]
        public static bool AddItem2Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result) {
            bool intercepted = InterceptAddItem(__instance, item, item.m_stack, new Vector2i(-1, -1), out bool successfulAdded);

            if (intercepted) {
                __result = successfulAdded;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.Changed)), HarmonyPostfix]
        public static void AddItemPostfix(Inventory __instance) {
            AssignItemsOfInventory(__instance);
        }

        private static void AssignItemsOfInventory(Inventory inventory) {
            foreach (ItemDrop.ItemData item in inventory.GetAllItems()) {
                InventoryOfItem.Remove(item);
                InventoryOfItem.Add(item, inventory);
            }
        }

        private static bool InterceptAddItem(Inventory inventory, ItemDrop.ItemData item, int amount, Vector2i pos, out bool successfulAdded) {
            InventoryOwner from = InventoryOwner.GetInventoryObjectOfItem(item);
            InventoryOwner to = InventoryOwner.GetInventoryObject(inventory);

            if (from == null || !from.IsValid() || to == null || !to.IsValid()) {
                successfulAdded = false;
                return false;
            }

            if (from.ZNetView.IsOwner() && !to.ZNetView.IsOwner()) {
                if (to is ContainerInventoryOwner toContainer) {
                    RequestAdd requestAdd = toContainer.Container.AddItemToChest(item, from.Inventory, pos, from.ZNetView.GetZDO().m_uid, amount, true);
                    successfulAdded = requestAdd.dragAmount == amount;
                    return true;
                }
            }

            if (!from.ZNetView.IsOwner() && to.ZNetView.IsOwner()) {
                if (from is ContainerInventoryOwner fromContainer) {
                    RequestRemove requestRemove = fromContainer.Container.RemoveItemFromChest(item, to.Inventory, pos, to.ZNetView.GetZDO().m_uid, amount, to.Inventory.GetItemAt(pos.x, pos.y));
                    successfulAdded = requestRemove.dragAmount == amount;
                    return true;
                }
            }

            if (!from.ZNetView.IsOwner() && !to.ZNetView.IsOwner()) {
                if (from is ContainerInventoryOwner fromContainer && to is ContainerInventoryOwner toContainer && fromContainer.Container == toContainer.Container) {
                    fromContainer.Container.MoveItemInChest(item.m_gridPos, pos, amount);
                    successfulAdded = true;
                    return true;
                }
            }

            successfulAdded = false;
            return false;
        }
    }
}