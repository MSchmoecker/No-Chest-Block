using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace NoChestBlock.Patches {
    [HarmonyPatch]
    public class InventoryGuiOnSelectedItemMovePatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnSelectedItemMovePatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            Label label = generator.DefineLabel();

            return new CodeMatcher(instructions, generator)
                   .MatchForward(true, new CodeMatch(i => CodeMatcherExtensions.IsVirtCall(i, nameof(InventoryGrid), nameof(InventoryGrid.DropItem))))
                   .Advance(-8)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1),
                                     new CodeInstruction(OpCodes.Ldarg_3),
                                     new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InventoryGuiOnSelectedItemMovePatch), nameof(MoveItem))),
                                     new CodeInstruction(OpCodes.Brfalse, label),
                                     new CodeInstruction(OpCodes.Ret))
                   .AddLabels(new[] { label })
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
    }
}
