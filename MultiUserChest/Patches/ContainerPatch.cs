using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace MultiUserChest.Patches {
    [HarmonyPatch]
    public static class ContainerPatch {
        public const string ItemMoveRPC = "MUC_RequestItemMove";
        public const string ItemMoveResponseRPC = "MUC_RequestItemMoveResponse";

        public const string ItemAddRPC = "MUC_RequestItemAdd";
        public const string ItemAddResponseRPC = "MUC_RequestItemAddResponse";

        public const string ItemRemoveRPC = "MUC_RequestItemRemove";
        public const string ItemRemoveResponseRPC = "MUC_RequestItemRemoveResponse";

        public const string ItemConsumeRPC = "MUC_RequestItemConsume";
        public const string ItemConsumeResponseRPC = "MUC_RequestItemConsumeResponse";

        public const string ItemDropRPC = "MUC_RequestItemDrop";
        public const string ItemDropResponseRPC = "MUC_RequestItemDropResponse";

        [HarmonyPatch(typeof(Container), nameof(Container.Awake)), HarmonyPostfix]
        public static void ContainerAwakePatch(Container __instance) {
            __instance.gameObject.AddComponent<ContainerExtend>();

            if (__instance.IsOdinShipContainer()) {
                return;
            }

            if (!__instance.m_nview) {
                __instance.m_nview = __instance.m_rootObjectOverride ? __instance.m_rootObjectOverride.GetComponent<ZNetView>() : __instance.GetComponent<ZNetView>();
            }
        }

        // This could maybe converted to a transpiler but is currently not worth it as the order of the statements have to be changed
        [HarmonyPatch(typeof(Container), nameof(Container.RPC_RequestOpen)), HarmonyPrefix]
        public static bool ContainerRPC_RequestOpenPatch(Container __instance, long uid, long playerID) {
            if (__instance.IsOdinShipContainer() || !__instance.m_nview.IsOwner()) {
                return true;
            }

            if (!__instance.CheckAccess(playerID)) {
                __instance.m_nview.InvokeRPC(uid, "OpenRespons", false);
                return false;
            }

            if (IsContainerInUse(__instance, uid)) {
                __instance.m_nview.InvokeRPC(uid, "OpenRespons", true);
                return false;
            }

            ZDOMan.instance.ForceSendZDO(uid, __instance.m_nview.GetZDO().m_uid);
            __instance.m_nview.GetZDO().SetOwner(uid);
            __instance.m_nview.InvokeRPC(uid, "OpenRespons", true);

            return false;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.RPC_RequestStack)), HarmonyPrefix]
        public static bool ContainerRPC_RequestStackPatch(Container __instance, long uid, long playerID) {
            if (__instance.IsOdinShipContainer() || !__instance.m_nview.IsOwner()) {
                return true;
            }

            if (!__instance.CheckAccess(playerID)) {
                __instance.m_nview.InvokeRPC(uid, "RPC_StackResponse", false);
                return false;
            }

            if (IsContainerInUse(__instance, uid)) {
                __instance.m_nview.InvokeRPC(uid, "RPC_StackResponse", true);
                return false;
            }

            ZDOMan.instance.ForceSendZDO(uid, __instance.m_nview.GetZDO().m_uid);
            __instance.m_nview.GetZDO().SetOwner(uid);
            __instance.m_nview.InvokeRPC(uid, "RPC_StackResponse", true);
            return false;
        }

        private static bool IsContainerInUse(Container container, long playerId) {
            bool containerUse = container.IsInUse();
            bool wagonUse = container.m_wagon && container.m_wagon.InUse();
            bool isMe = playerId == ZNet.GetUID();
            return (containerUse || wagonUse) && !isMe;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.UpdateUseVisual)), HarmonyPostfix]
        public static void ContainerUpdateUseVisualPatch(Container __instance) {
            if (__instance.IsOdinShipContainer()) {
                return;
            }

            if (!__instance.m_nview.IsValid() || !Player.m_localPlayer || !InventoryGui.instance) {
                return;
            }

            bool inUse = __instance == InventoryGui.instance.m_currentContainer || IsContainerInUse(__instance);

            if (__instance.m_open) {
                __instance.m_open.SetActive(inUse);
            }

            if (__instance.m_closed) {
                __instance.m_closed.SetActive(!inUse);
            }
        }

        private static bool IsContainerInUse(Container container) {
            ZDOID containerId = container.m_nview.GetZDO().m_uid;
            return Player.GetAllPlayers().Any(p => p != Player.m_localPlayer && p.m_nview.IsValid() && p.m_nview.GetZDO().GetZDOID("accessed-container") == containerId);
        }
    }
}
