using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace NoChestBlock.Patches {
    [HarmonyPatch]
    public class InventoryGuiPatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update)), HarmonyPrefix]
        public static void InventoryGuiUpdatePatch(InventoryGui __instance) {
            if (__instance.m_currentContainer) {
                __instance.m_currentContainer.CheckForChanges();
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTakeAll)), HarmonyPrefix]
        public static bool InventoryGuiTakeAllPatch(InventoryGui __instance) {
            if (Player.m_localPlayer.IsTeleporting() || !__instance.m_currentContainer) {
                return false;
            }

            if (__instance.m_currentContainer.IsOwner()) {
                return true;
            }

            ContainerHandler.TakeAll(__instance.m_currentContainer);
            return false;
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnRightClickItem)), HarmonyPrefix]
        public static bool InventoryGuiOnRightClickItemPatch(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item) {
            if (item == null || !Player.m_localPlayer) {
                return true;
            }

            if (grid.GetInventory() == Player.m_localPlayer.GetInventory()) {
                return true;
            }

            if (!__instance.m_currentContainer || __instance.m_currentContainer.IsOwner()) {
                return true;
            }

            if (InventoryHandler.blockConsume) {
                return false;
            }

            if (Player.m_localPlayer.CanConsumeItem(item)) {
                InventoryHandler.blockConsume = true;
                RequestConsume request = new RequestConsume(item);

                Timer.Start(request);
                __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemConsume", request.WriteToPackage());
            }

            return false;
        }

        // Remove IsOwner() check
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainer)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateContainerPatch(IEnumerable<CodeInstruction> instructions) {
            return new CodeMatcher(instructions)
                   .MatchForward(false,
                                 new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "m_currentContainer"),
                                 new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "IsOwner"))
                   .RemoveInstructions(4)
                   .InstructionEnumeration();
        }

        private static bool IsVirtCall(CodeInstruction i, string declaringType, string name) {
            return i.opcode == OpCodes.Callvirt && i.operand is MethodInfo methodInfo && methodInfo.DeclaringType?.Name == declaringType && methodInfo.Name == name;
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnSelectedItemPatch(IEnumerable<CodeInstruction> instructions) {
            Label label = new Label();

            return new CodeMatcher(instructions)
                   .MatchForward(true, new CodeMatch(i => IsVirtCall(i, nameof(InventoryGrid), nameof(InventoryGrid.DropItem))))
                   .Advance(-8)
                   .InsertAndAdvance(
                                     new CodeInstruction(OpCodes.Ldarg_1),
                                     new CodeInstruction(OpCodes.Ldarg_3),
                                     new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InventoryGuiPatch), nameof(MoveItem))),
                                     new CodeInstruction(OpCodes.Brfalse, label),
                                     new CodeInstruction(OpCodes.Ret))
                   .AddLabels(new[] { label })
                   .MatchForward(false,
                                 new CodeMatch(i => IsVirtCall(i, nameof(Humanoid), nameof(Humanoid.UnequipItem))),
                                 new CodeMatch(OpCodes.Ldarg_1),
                                 new CodeMatch(i => IsVirtCall(i, nameof(InventoryGrid), nameof(InventoryGrid.GetInventory))))
                   .Advance(1)
                   .InsertAndAdvance(
                                     new CodeInstruction(OpCodes.Ldarg_1),
                                     new CodeInstruction(OpCodes.Ldarg_2),
                                     new CodeInstruction(OpCodes.Ldarg_3),
                                     new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InventoryGuiPatch), nameof(FastMoveItem))),
                                     new CodeInstruction(OpCodes.Ret))
                   .InstructionEnumeration();
        }

        private static bool MoveItem(InventoryGrid grid, Vector2i toPos) {
            InventoryGui gui = InventoryGui.instance;
            Player player = Player.m_localPlayer;

            bool isOwnerOfContainer = gui.m_currentContainer && gui.m_currentContainer.IsOwner();
            bool isPlayerInventory = grid.m_inventory == player.m_inventory && gui.m_dragInventory == player.m_inventory;

            if (isOwnerOfContainer || isPlayerInventory) {
                Log.LogInfo("MoveItem in own inventory");
                return false;
            }

            Vector2i fromPos = gui.m_dragItem.m_gridPos;
            int dragAmount = gui.m_dragAmount;

            if (grid.GetInventory() == gui.m_dragInventory) {
                RequestMove request = new RequestMove(fromPos, toPos, dragAmount);
                Timer.Start(request);

                gui.m_currentContainer.m_nview.InvokeRPC("RequestItemMove", request.WriteToPackage());
            } else if (grid.m_inventory == gui.m_currentContainer.GetInventory()) {
                ContainerHandler.AddItemToChest(fromPos, toPos, dragAmount, true, player.GetInventory(), gui.m_currentContainer);
            } else {
                ItemDrop.ItemData prevItem = grid.GetInventory().GetItemAt(toPos.x, toPos.y);
                RequestRemove request = new RequestRemove(fromPos, toPos, dragAmount, prevItem);
                ContainerHandler.RemoveItemFromChest(request, gui.m_currentContainer);
            }

            Log.LogDebug("MoveItem in other inventory");
            return true;
        }

        private static void FastMoveItem(InventoryGrid grid, ItemDrop.ItemData item, Vector2i toPos) {
            InventoryGui gui = InventoryGui.instance;
            Player player = Player.m_localPlayer;

            if (grid.GetInventory() == gui.m_currentContainer.GetInventory()) {
                if (!player.GetInventory().CanAddItem(item)) {
                    return;
                }

                Vector2i targetSlot = InventoryHelper.FindEmptySlot(player.GetInventory(), InventoryHandler.blockedSlots);

                if (targetSlot.x == -1 || targetSlot.y == -1) {
                    return;
                }

                RequestRemove request = new RequestRemove(toPos, targetSlot, item.m_stack, null);
                ContainerHandler.RemoveItemFromChest(request, gui.m_currentContainer);
            } else {
                Vector2i targetPos = gui.m_currentContainer.GetInventory().FindEmptySlot(true);
                ContainerHandler.AddItemToChest(toPos, targetPos, item.m_stack, false, player.GetInventory(), gui.m_currentContainer);
            }

            gui.m_moveItemEffects.Create(gui.transform.position, Quaternion.identity);
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnDropOutside)), HarmonyPrefix]
        public static bool InventoryGuiOnDropOutsidePatch() {
            InventoryGui gui = InventoryGui.instance;
            Player player = Player.m_localPlayer;

            if (!gui.m_dragGo) {
                return false;
            }

            bool isOwnerOfContainer = gui.m_currentContainer && gui.m_currentContainer.IsOwner();
            bool isPlayerInventory = gui.m_dragInventory == player.m_inventory;

            if (isOwnerOfContainer || isPlayerInventory) {
                Log.LogInfo("Drop item from own inventory");
                return true;
            }

            RequestRemove request = new RequestRemove(gui.m_dragItem.m_gridPos, new Vector2i(-1, -1), gui.m_dragItem.m_stack, null);
            ContainerHandler.RemoveItemFromChest(request, gui.m_currentContainer);
            return false;
        }
    }
}
