using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace MultiUserChest.Patches {
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
            Player player = Player.m_localPlayer;

            if (item == null || !player) {
                return true;
            }

            if (grid.GetInventory() == player.GetInventory()) {
                return true;
            }

            if (!__instance.m_currentContainer || __instance.m_currentContainer.IsOwner()) {
                return true;
            }

            if (InventoryBlock.Get(player.GetInventory()).IsConsumeBlocked()) {
                return false;
            }

            if (player.CanConsumeItem(item)) {
                InventoryBlock.Get(player.GetInventory()).BlockConsume(true);
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

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnDropOutside)), HarmonyPrefix]
        public static bool InventoryGuiOnDropOutsidePatch() {
            InventoryGui gui = InventoryGui.instance;
            Player player = Player.m_localPlayer;

            if (!gui.m_dragGo) {
                return false;
            }

            bool isOwnerOfContainer = gui.m_currentContainer && gui.m_currentContainer.IsOwner();
            bool isPlayerInventory = gui.m_dragInventory == player.GetInventory();

            if (isOwnerOfContainer || isPlayerInventory) {
                Log.LogDebug("Drop item from own inventory");
                return true;
            }

            RequestDrop request = new RequestDrop(gui.m_dragItem.m_gridPos, gui.m_dragAmount, player.GetZDOID());
            Timer.Start(request);

            gui.m_currentContainer.m_nview.InvokeRPC("RequestDropItems", request.WriteToPackage());
            return false;
        }
    }
}
