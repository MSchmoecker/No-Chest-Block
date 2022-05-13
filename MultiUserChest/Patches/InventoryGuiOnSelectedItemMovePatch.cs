using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace MultiUserChest.Patches {
    [HarmonyPatch]
    public class InventoryGuiOnSelectedItemMovePatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnSelectedItemMovePatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            Label label = generator.DefineLabel();

            return new CodeMatcher(instructions, generator)
                   .MatchForward(true, new CodeMatch(i => CodeMatcherExtensions.IsVirtCall(i, nameof(InventoryGrid), nameof(InventoryGrid.DropItem))))
                   .Advance(-8)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1),
                                     new CodeInstruction(OpCodes.Ldarg_2),
                                     new CodeInstruction(OpCodes.Ldarg_3),
                                     new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InventoryGuiOnSelectedItemMovePatch), nameof(MoveItem))),
                                     new CodeInstruction(OpCodes.Brtrue, label),
                                     new CodeInstruction(OpCodes.Ret))
                   .AddLabels(new[] { label })
                   .InstructionEnumeration();
        }

        private static bool MoveItem(InventoryGrid grid, ItemDrop.ItemData item, Vector2i toPos) {
            Log.LogDebug("MoveItem");

            InventoryGui gui = InventoryGui.instance;

            bool isOwnerOfContainer = gui.m_currentContainer && gui.m_currentContainer.IsOwner();
            bool isPlayerInventory = IsPlayerInventory(grid, gui);

            if (isOwnerOfContainer || isPlayerInventory) {
                Log.LogDebug("MoveItem in own inventory");
                return true;
            }

            Vector2i fromPos = gui.m_dragItem.m_gridPos;
            int dragAmount = gui.m_dragAmount;

            if (grid.GetInventory() == gui.m_dragInventory) {
                RequestMove request = new RequestMove(fromPos, toPos, dragAmount);
                Timer.Start(request);

                gui.m_currentContainer.m_nview.InvokeRPC("RequestItemMove", request.WriteToPackage());
            } else if (grid.GetInventory() == gui.m_currentContainer.GetInventory()) {
                gui.m_currentContainer.AddItemToChest(item, gui.m_dragInventory, toPos, Player.m_localPlayer.GetZDOID(), dragAmount, true);
            } else {
                ItemDrop.ItemData prevItem = grid.GetInventory().GetItemAt(toPos.x, toPos.y);
                ZDOID zdoid = Player.m_localPlayer.GetZDOID();
                gui.m_currentContainer.RemoveItemFromChest(gui.m_dragItem, grid.GetInventory(), toPos, zdoid, dragAmount, prevItem);
            }

            gui.SetupDragItem(null, null, 1);

            Log.LogDebug("MoveItem in other inventory");
            return false;
        }

        private static bool IsPlayerInventory(InventoryGrid grid, InventoryGui gui) {
            Player player = Player.m_localPlayer;
            bool fromPlayer = gui.m_dragInventory == player.GetInventory();
            bool toPlayer = grid.GetInventory() == player.GetInventory();

            if (fromPlayer && toPlayer) {
                return true;
            }

            if (player.GetInventory().IsType("ExtendedInventory") && player.GetInventory().HasField("_inventories")) {
                List<Inventory> inventories = player.GetInventory().GetField<List<Inventory>>("_inventories");
                fromPlayer = inventories.Any(i => gui.m_dragInventory == i);
                toPlayer = inventories.Any(i => grid.GetInventory() == i);
            }

            return fromPlayer && toPlayer;
        }
    }
}
