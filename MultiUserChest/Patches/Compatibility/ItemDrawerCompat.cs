using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace MultiUserChest {
    public static class ItemDrawerCompat {
        private static Type drawerContainerType = AccessTools.TypeByName("DrawerContainer, itemdrawers");
        private static AccessTools.FieldRef<object, ItemDrop.ItemData> drawerField_item;

        private static AccessTools.FieldRef<object, ItemDrop.ItemData> DrawerField_item {
            get {
                if (drawerField_item == null) {
                    drawerField_item = AccessTools.FieldRefAccess<ItemDrop.ItemData>(drawerContainerType, "_item");
                }

                return drawerField_item;
            }
        }

        public static bool IsItemDrawer(Container container) {
            return drawerContainerType != null && container.GetComponent(drawerContainerType);
        }

        [HarmonyPatch("DrawerContainer, itemdrawers", "OnInventoryChanged"), HarmonyPrefix]
        public static void UpdateItemOfDrawer(Container __instance) {
            if (__instance.m_inventory.m_inventory.Count == 0) {
                return;
            }

            if (__instance.m_inventory.m_inventory[0] != null) {
                DrawerField_item(__instance) = __instance.m_inventory.m_inventory[0].Clone();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData) })]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPriority(Priority.VeryHigh)]
        public static bool PreventItemAdd(Inventory __instance, ItemDrop.ItemData item) {
            if (InventoryOwner.GetOwner(__instance) is ContainerInventoryOwner containerOwner && containerOwner.IsItemDrawer) {
                ItemDrop.ItemData drawerItem = DrawerField_item(containerOwner.Container);

                if (drawerItem != null && drawerItem.PrefabName() != item?.PrefabName()) {
                    Log.LogInfo($"PreventItemAdd: {drawerItem.PrefabName()} != {item?.PrefabName()}");
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.CanAddItem), typeof(ItemDrop.ItemData), typeof(int)), HarmonyPostfix]
        public static void CanAddItem(Inventory __instance, ref bool __result, ItemDrop.ItemData item) {
            if (__result && InventoryOwner.GetOwner(__instance) is ContainerInventoryOwner containerOwner && containerOwner.IsItemDrawer) {
                ItemDrop.ItemData drawerItem = DrawerField_item(containerOwner.Container);
                __result = drawerItem == null || drawerItem.PrefabName() == item?.PrefabName();
            }
        }

        [HarmonyPatch("DrawerContainer, itemdrawers", "AddItem"), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CloneItemBeforeAssignmentTranspiler(IEnumerable<CodeInstruction> instructions) {
            return new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemDrop), nameof(ItemDrop.m_itemData))),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(drawerContainerType, "_item"))
                )
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InventoryHelper), nameof(InventoryHelper.CloneDeeper))))
                .InstructionEnumeration();
        }
    }
}
