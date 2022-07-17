﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace MultiUserChest.Patches.Compatibility {
    public class QuickStackPatch {
        [HarmonyPatch("QuickStack.QuickStackPlugin, QuickStack", "DoQuickStack"), HarmonyPrefix]
        public static bool DoQuickStackPatch(Player player) {
            if (player.IsTeleporting()) {
                return false;
            }

            Container container = InventoryGui.instance.m_currentContainer;

            if (container && container.m_nview.IsValid()) {
                int moved = CallStackItems(player, container, 0);
                ReportResult(player, moved);
                return false;
            }

            List<Container> nearbyContainers = FindNearbyContainers(player.transform.position);
            if (nearbyContainers.Count != 0) {
                StackToMany(player, nearbyContainers);
            }

            return false;
        }

        private static int CallStackItems(Player player, Container container, int moved) {
            long playerID = player.GetPlayerID();
            Inventory playerInventory = player.GetInventory();

            if (container.IsOwner()) {
                moved += StackItems(playerInventory, container.GetInventory(), playerID);
            } else {
                void StackItemsNoReturn(Inventory from, Inventory to) {
                    StackItems(@from, to, playerID);
                }

                List<ItemDrop.ItemData> movable = InventoryHelper.GetAllMoveableItems(playerInventory, container.GetInventory(), StackItemsNoReturn);
                moved += movable.Count;

                foreach (ItemDrop.ItemData item in movable) {
                    container.AddItemToChest(item, playerInventory, new Vector2i(-1, -1), player.GetZDOID(), item.m_stack, true);
                }
            }

            return moved;
        }

        private static void StackToMany(Player player, List<Container> containers) {
            ReflectionHelper.InvokeStaticMethod<object>("QuickStack.QuickStackPlugin, QuickStack", "StackToMany", player, containers);
        }

        private static void ReportResult(Player player, int moved) {
            ReflectionHelper.InvokeStaticMethod<object>("QuickStack.QuickStackPlugin, QuickStack", "reportResult", player, moved);
        }

        private static int StackItems(Inventory from, Inventory to, long playerID) {
            return ReflectionHelper.InvokeStaticMethod<int>("QuickStack.QuickStackPlugin, QuickStack", "StackItems", playerID, from, to);
        }

        private static List<Container> FindNearbyContainers(Vector3 point) {
            return ReflectionHelper.InvokeStaticMethod<List<Container>>("QuickStack.QuickStackPlugin, QuickStack", "FindNearbyContainers", point);
        }
    }
}
