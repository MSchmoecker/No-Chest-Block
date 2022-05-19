using HarmonyLib;

namespace MultiUserChest.Patches {
    /// <summary>
    ///     block complete pickup if any slot is blocked
    ///     slots are only blocked a limited amount of time and changed inventories (EAQS) don't have to be taken into account this way
    /// </summary>
    [HarmonyPatch]
    public static class PickupPatch {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveEmptySlot)), HarmonyPrefix, HarmonyPriority(Priority.VeryLow)]
        public static void HaveEmptySlotPrefix(Inventory __instance, ref bool __result) {
            __result = __result && !InventoryBlock.Get(__instance).IsAnySlotBlocked();
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveEmptySlot)), HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void HaveEmptySlotPostfix(Inventory __instance, ref bool __result) {
            __result = __result && !InventoryBlock.Get(__instance).IsAnySlotBlocked();
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindEmptySlot)), HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void FindEmptySlotPostfix(Inventory __instance, ref Vector2i __result) {
            if (InventoryBlock.Get(__instance).IsSlotBlocked(__result)) {
                __result = new Vector2i(-1, -1);
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindFreeStackItem)), HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void FindFreeStackItemPostfix(Inventory __instance, ref ItemDrop.ItemData __result) {
            if (__result != null && InventoryBlock.Get(__instance).IsSlotBlocked(__result.m_gridPos)) {
                __result = null;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindFreeStackSpace)), HarmonyPrefix, HarmonyPriority(Priority.VeryLow)]
        public static void FindFreeStackSpacePrefix(Inventory __instance, ref int __result) {
            if (InventoryBlock.Get(__instance).IsAnySlotBlocked()) {
                __result = 0;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindFreeStackSpace)), HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void FindFreeStackSpacePostfix(Inventory __instance, ref int __result) {
            if (InventoryBlock.Get(__instance).IsAnySlotBlocked()) {
                __result = 0;
            }
        }
    }
}
