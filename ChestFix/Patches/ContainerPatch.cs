using System;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using UnityEngine;

namespace ChestFix.Patches {
    [HarmonyPatch]
    public class ContainerPatch {
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static readonly List<Vector2i> blockedInventorySlots = new List<Vector2i>();

        [HarmonyPatch(typeof(Container), nameof(Container.IsInUse)), HarmonyPostfix]
        public static void IsInUsePatch(ref bool __result, ref bool ___m_inUse) {
            __result = false;
            ___m_inUse = false;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.Awake)), HarmonyPostfix]
        public static void ContainerAwakePatch(Container __instance) {
            __instance.m_nview.Register<ZPackage>("RequestItemMove", (l, package) => __instance.RPC_RequestItemMove(l, package));
            __instance.m_nview.Register<ZPackage>("RequestItemAdd", (l, package) => RPC_RequestItemAdd(__instance, l, package));
            __instance.m_nview.Register<ZPackage>("RequestItemRemove", (l, package) => RPC_RequestItemRemove(__instance, l, package));

            __instance.m_nview.Register<bool>("RequestItemMoveResponse", RPC_RequestItemMoveResponse);
            __instance.m_nview.Register<ZPackage>("RequestItemAddResponse", RPC_RequestItemAddResponse);
            __instance.m_nview.Register<ZPackage>("RequestItemRemoveResponse", RPC_RequestItemRemoveResponse);
        }

        private static void RPC_RequestItemMoveResponse(long sender, bool success) {
            stopwatch.Stop();
            Log.LogInfo($"RPC_RequestItemMoveResponse: {stopwatch.ElapsedMilliseconds}ms, success: {success}");


            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        private static void RPC_RequestItemAdd(Container container, long l, ZPackage package) {
            ZPackage response = container.GetInventory().RPC_RequestItemAdd(l, package);
            container.m_nview.InvokeRPC(l, "RequestItemAddResponse", response);
        }

        private static void RPC_RequestItemRemove(Container container, long l, ZPackage package) {
            ZPackage response = container.GetInventory().RPC_RequestItemRemove(l, package);
            container.m_nview.InvokeRPC(l, "RequestItemRemoveResponse", response);
        }

        private static void RPC_RequestItemRemoveResponse(long sender, ZPackage package) {
            stopwatch.Stop();
            Log.LogInfo($"RPC_RequestItemRemoveResponse: {stopwatch.ElapsedMilliseconds}ms");

            bool success = package.ReadBool();
            int amount = package.ReadInt();
            bool hasSwitched = package.ReadBool();
            Vector2i inventoryPos = package.ReadVector2i();
            bool hasResponseItem = package.ReadBool();
            ItemDrop.ItemData responseItem = null;

            if (hasResponseItem) {
                responseItem = InventoryHelper.LoadItemFromPackage(package, inventoryPos);
            }

            Inventory playerInv = Player.m_localPlayer.GetInventory();

            if (blockedInventorySlots.Contains(inventoryPos)) {
                blockedInventorySlots.Remove(inventoryPos);
            }

            if (success) {
                if (hasSwitched) {
                    ItemDrop.ItemData atSlot = playerInv.GetItemAt(inventoryPos.x, inventoryPos.y);
                    playerInv.RemoveItem(atSlot);
                }

                playerInv.AddItem(responseItem, amount, inventoryPos.x, inventoryPos.y);
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        private static void RPC_RequestItemAddResponse(long sender, ZPackage package) {
            stopwatch.Stop();
            Log.LogInfo($"RPC_RequestItemAddResponse: {stopwatch.ElapsedMilliseconds}ms");

            Vector2i inventoryPos = package.ReadVector2i();
            bool success = package.ReadBool();
            int amount = package.ReadInt();
            bool hasSwitched = package.ReadBool();

            Log.LogInfo($"success: {success}");
            Log.LogInfo($"amount: {amount}");
            Log.LogInfo($"hasSwitched: {hasSwitched}");

            if (success) {
                ItemDrop.ItemData toRemove = Player.m_localPlayer.GetInventory().GetItemAt(inventoryPos.x, inventoryPos.y);
                Player.m_localPlayer.GetInventory().RemoveItem(toRemove, amount);

                if (hasSwitched) {
                    InventoryHelper.LoadItemIntoInventory(package, Player.m_localPlayer.GetInventory(), inventoryPos, -1, -1);
                }
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update)), HarmonyPrefix]
        public static void InventoryGuiUpdatePatch(InventoryGui __instance) {
            if (__instance.m_currentContainer) {
                __instance.m_currentContainer.CheckForChanges();
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainer)), HarmonyPrefix]
        public static bool UpdateContainerPatch(InventoryGui __instance, Player player) {
            if (!__instance.m_animator.GetBool("visible")) {
                return false;
            }

            if ((bool)__instance.m_currentContainer /*&& __instance.m_currentContainer.IsOwner()*/) {
                __instance.m_currentContainer.SetInUse(inUse: true);
                __instance.m_container.gameObject.SetActive(value: true);
                __instance.m_containerGrid.UpdateInventory(__instance.m_currentContainer.GetInventory(), null,
                                                           __instance.m_dragItem);
                __instance.m_containerName.text = Localization.instance.Localize(__instance.m_currentContainer.GetInventory().GetName());
                if (__instance.m_firstContainerUpdate) {
                    __instance.m_containerGrid.ResetView();
                    __instance.m_firstContainerUpdate = false;
                }

                if (Vector3.Distance(__instance.m_currentContainer.transform.position, player.transform.position) >
                    __instance.m_autoCloseDistance) {
                    __instance.CloseContainer();
                }
            } else {
                __instance.m_container.gameObject.SetActive(value: false);
                if (__instance.m_dragInventory != null && __instance.m_dragInventory != Player.m_localPlayer.GetInventory()) {
                    __instance.SetupDragItem(null, null, 1);
                }
            }

            return false;
        }

        /*[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyPrefix]
        public static void OnSelectedItemPatch(InventoryGui __instance) {
            if (!__instance.m_currentContainer.IsOwner()) {
                Logger.LogInfo("Claimed ownership for inventory!");
                long owner = __instance.m_currentContainer.m_nview.GetZDO().m_owner;
                __instance.m_currentContainer.m_nview.ClaimOwnership();
                ZDOMan.instance.ForceSendZDO(owner, __instance.m_currentContainer.m_nview.GetZDO().m_uid);

            }
        }*/

        /*[HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem)), HarmonyPrefix]
        public static void OnSelectedItemPatch(InventoryGrid __instance) {
            if (!__instance.m_inventory.IsOwner()) {
                __instance.dr
                Logger.LogInfo("Claimed ownership for inventory!");
                long owner = __instance.m_currentContainer.m_nview.GetZDO().m_owner;
                __instance.m_currentContainer.m_nview.ClaimOwnership();
                ZDOMan.instance.ForceSendZDO(owner, __instance.m_currentContainer.m_nview.GetZDO().m_uid);
            }
        }*/

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyPrefix]
        public static bool OnSelectedItemPatch(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos,
            InventoryGrid.Modifier mod) {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer.IsTeleporting()) {
                return false;
            }

            if ((bool)__instance.m_dragGo) {
                __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
                bool flag = localPlayer.IsItemEquiped(__instance.m_dragItem);
                bool flag2 = item != null && localPlayer.IsItemEquiped(item);
                Vector2i gridPos = __instance.m_dragItem.m_gridPos;
                if ((__instance.m_dragItem.m_shared.m_questItem || (item != null && item.m_shared.m_questItem)) &&
                    __instance.m_dragInventory != grid.GetInventory()) {
                    return false;
                }

                if (!__instance.m_dragInventory.ContainsItem(__instance.m_dragItem)) {
                    __instance.SetupDragItem(null, null, 1);
                    return false;
                }

                localPlayer.RemoveFromEquipQueue(item);
                localPlayer.RemoveFromEquipQueue(__instance.m_dragItem);
                localPlayer.UnequipItem(__instance.m_dragItem, triggerEquipEffects: false);
                localPlayer.UnequipItem(item, triggerEquipEffects: false);

                if (!__instance.m_currentContainer.IsOwner() &&
                    !(grid.GetInventory() == localPlayer.m_inventory && __instance.m_dragInventory == localPlayer.m_inventory)) {
                    // Logger.LogInfo($"from container {__instance.m_currentContainer.m_nview.GetZDO().m_uid}");
                    // Logger.LogInfo($"m_dragItem pos {__instance.m_dragItem.m_gridPos}");
                    // Logger.LogInfo($"pos {pos}");
                    // Logger.LogInfo($"m_dragAmount {__instance.m_dragAmount}");

                    if (grid.GetInventory() == __instance.m_dragInventory) {
                        ZPackage data = new ZPackage();
                        data.Write(__instance.m_dragItem.m_gridPos);
                        data.Write(pos);
                        data.Write(__instance.m_dragAmount);

                        Log.LogInfo("RequestItemMove");
                        stopwatch.Reset();
                        stopwatch.Start();
                        __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemMove", data);
                    } else if (grid.m_inventory == __instance.m_currentContainer.GetInventory()) {
                        ZPackage data = new ZPackage();
                        data.Write(__instance.m_dragItem.m_gridPos);
                        data.Write(pos);
                        data.Write(__instance.m_dragAmount);
                        InventoryHelper.WriteItemToPackage(__instance.m_dragItem, data);
                        data.Write(true);

                        Log.LogInfo("RequestItemAdd");
                        stopwatch.Reset();
                        stopwatch.Start();
                        __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemAdd", data);
                    } else {
                        ItemDrop.ItemData prevItem = grid.GetInventory().GetItemAt(pos.x, pos.y);

                        ZPackage data = new ZPackage();
                        data.Write(__instance.m_dragItem.m_gridPos);
                        data.Write(pos);
                        data.Write(__instance.m_dragAmount);
                        data.Write(prevItem != null);

                        if (prevItem != null) {
                            InventoryHelper.WriteItemToPackage(prevItem, data);
                        }

                        blockedInventorySlots.AddItem(__instance.m_dragItem.m_gridPos);

                        Log.LogInfo("RequestItemRemove");
                        stopwatch.Reset();
                        stopwatch.Start();
                        __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemRemove", data);
                    }

                    return false;
                }

                Log.LogInfo("Move inside own inventory");
                bool num = grid.DropItem(__instance.m_dragInventory, __instance.m_dragItem, __instance.m_dragAmount, pos);
                if (__instance.m_dragItem.m_stack < __instance.m_dragAmount) {
                    __instance.m_dragAmount = __instance.m_dragItem.m_stack;
                }

                if (flag) {
                    ItemDrop.ItemData itemAt = grid.GetInventory().GetItemAt(pos.x, pos.y);
                    if (itemAt != null) {
                        localPlayer.EquipItem(itemAt, triggerEquipEffects: false);
                    }

                    if (localPlayer.GetInventory().ContainsItem(__instance.m_dragItem)) {
                        localPlayer.EquipItem(__instance.m_dragItem, triggerEquipEffects: false);
                    }
                }

                if (flag2) {
                    ItemDrop.ItemData itemAt2 = __instance.m_dragInventory.GetItemAt(gridPos.x, gridPos.y);
                    if (itemAt2 != null) {
                        localPlayer.EquipItem(itemAt2, triggerEquipEffects: false);
                    }

                    if (localPlayer.GetInventory().ContainsItem(item)) {
                        localPlayer.EquipItem(item, triggerEquipEffects: false);
                    }
                }

                if (num) {
                    __instance.SetupDragItem(null, null, 1);
                    __instance.UpdateCraftingPanel();
                }
            } else {
                if (item == null) {
                    return false;
                }

                switch (mod) {
                    case InventoryGrid.Modifier.Move:
                        if (item.m_shared.m_questItem) {
                            return false;
                        }

                        if (__instance.m_currentContainer != null) {
                            localPlayer.RemoveFromEquipQueue(item);
                            localPlayer.UnequipItem(item);

                            if (grid.GetInventory() == __instance.m_currentContainer.GetInventory()) {
                                if (localPlayer.GetInventory().CanAddItem(item)) {
                                    Vector2i targetSlot = InventoryHelper.FindEmptySlot(localPlayer.GetInventory(), blockedInventorySlots);

                                    if (targetSlot.x != -1 && targetSlot.y != -1) {
                                        ZPackage data = new ZPackage();
                                        data.Write(pos);
                                        data.Write(targetSlot);
                                        data.Write(item.m_stack);
                                        data.Write(false); // don't allow switch

                                        blockedInventorySlots.AddItem(targetSlot);

                                        Log.LogInfo("RequestItemRemove");
                                        stopwatch.Reset();
                                        stopwatch.Start();
                                        __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemRemove", data);
                                    }
                                }
                            } else {
                                ZPackage data = new ZPackage();
                                data.Write(pos);
                                data.Write(__instance.m_currentContainer.GetInventory().FindEmptySlot(true));
                                data.Write(item.m_stack);
                                InventoryHelper.WriteItemToPackage(item, data);
                                data.Write(false);

                                Log.LogInfo("RequestItemAdd");
                                stopwatch.Reset();
                                stopwatch.Start();
                                __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemAdd", data);
                            }

                            __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
                        } else if (Player.m_localPlayer.DropItem(localPlayer.GetInventory(), item, item.m_stack)) {
                            __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
                        }

                        return false;
                    case InventoryGrid.Modifier.Split:
                        if (item.m_stack > 1) {
                            __instance.ShowSplitDialog(item, grid.GetInventory());
                            return false;
                        }

                        break;
                }

                __instance.SetupDragItem(item, grid.GetInventory(), item.m_stack);
            }

            return false;
        }
    }
}
