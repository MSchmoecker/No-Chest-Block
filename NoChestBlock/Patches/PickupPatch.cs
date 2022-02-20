using HarmonyLib;

namespace NoChestBlock.Patches {
    /// <summary>
    ///     block complete pickup if any slot is blocked
    ///     slots are only blocked a limited amount of time and changed inventories (EAQS) don't have to be taken into account this way
    /// </summary>
    [HarmonyPatch]
    public static class PickupPatch {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveEmptySlot)), HarmonyPrefix, HarmonyPriority(Priority.VeryLow)]
        public static void HaveEmptySlotPrefix(ref bool __result) {
            __result = __result && !InventoryHandler.IsAnySlotBlocked();
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveEmptySlot)), HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void HaveEmptySlotPostfix(ref bool __result) {
            __result = __result && !InventoryHandler.IsAnySlotBlocked();
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindFreeStackSpace)), HarmonyPrefix, HarmonyPriority(Priority.VeryLow)]
        public static void FindFreeStackSpacePrefix(ref int __result) {
            if (InventoryHandler.IsAnySlotBlocked()) {
                __result = 0;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindFreeStackSpace)), HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void FindFreeStackSpacePostfix(ref int __result) {
            if (InventoryHandler.IsAnySlotBlocked()) {
                __result = 0;
            }
        }
    }
}
