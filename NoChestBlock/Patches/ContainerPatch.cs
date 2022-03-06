using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace NoChestBlock.Patches {
    [HarmonyPatch]
    public static class ContainerPatch {
        [HarmonyPatch(typeof(Container), nameof(Container.IsInUse)), HarmonyPostfix]
        public static void IsInUsePatch(ref bool __result, ref bool ___m_inUse) {
            __result = false;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.Awake)), HarmonyPostfix]
        public static void ContainerAwakePatch(Container __instance) {
            if (!__instance.m_nview) {
                __instance.m_nview = __instance.m_rootObjectOverride ? __instance.m_rootObjectOverride.GetComponent<ZNetView>() : __instance.GetComponent<ZNetView>();
            }

            __instance.RegisterRPCs();
        }

        public static void RegisterRPCs(this Container __instance) {
            ZNetView nview = __instance.m_nview;

            nview.Register<ZPackage>("RequestItemMove", (l, package) => ContainerRPCHandler.RPC_RequestItemMove(__instance, l, package));
            nview.Register<ZPackage>("RequestItemAdd", (l, package) => ContainerRPCHandler.RPC_RequestItemAdd(__instance, l, package));
            nview.Register<ZPackage>("RequestItemRemove", (l, package) => ContainerRPCHandler.RPC_RequestItemRemove(__instance, l, package));
            nview.Register<ZPackage>("RequestItemConsume", (l, package) => ContainerRPCHandler.RPC_RequestItemConsume(__instance, l, package));
            nview.Register<ZPackage>("RequestTakeAllItems", (l, package) => ContainerRPCHandler.RPC_RequestTakeAllItems(__instance, l, package));
            nview.Register<ZPackage>("RequestDropItems", (l, package) => ContainerRPCHandler.RPC_RequestDrop(__instance, l, package));

            nview.Register<bool>("RequestItemMoveResponse", InventoryHandler.RPC_RequestItemMoveResponse);
            nview.Register<ZPackage>("RequestItemAddResponse", InventoryHandler.RPC_RequestItemAddResponse);
            nview.Register<ZPackage>("RequestItemRemoveResponse", InventoryHandler.RPC_RequestItemRemoveResponse);
            nview.Register<ZPackage>("RequestItemConsumeResponse", InventoryHandler.RPC_RequestItemConsumeResponse);
            nview.Register<ZPackage>("RequestTakeAllItemsResponse", (l, package) => InventoryHandler.RPC_RequestTakeAllItemsResponse(__instance, l, package));
            nview.Register<ZPackage>("RequestDropResponse", InventoryHandler.RPC_RequestDropResponse);
        }

        // This could maybe converted to a transpiler but is currently not worth it as the order of the statements have to be changed
        [HarmonyPatch(typeof(Container), nameof(Container.RPC_RequestOpen)), HarmonyPrefix]
        public static bool ContainerRPC_RequestOpenPatch(Container __instance, long uid, long playerID) {
            if (!__instance.m_nview.IsOwner()) {
                return false;
            }

            if (!__instance.CheckAccess(playerID)) {
                ZLog.Log("  not yours");
                __instance.m_nview.InvokeRPC(uid, "OpenRespons", false);
            }

            bool containerUse = __instance.m_inUse;
            bool wagonUse = __instance.m_wagon && __instance.m_wagon.m_container && __instance.m_wagon.m_container.m_inUse;

            if ((containerUse || wagonUse) && uid != ZNet.instance.GetUID()) {
                __instance.m_nview.InvokeRPC(uid, "OpenRespons", true);
                return false;
            }

            ZDOMan.instance.ForceSendZDO(uid, __instance.m_nview.GetZDO().m_uid);
            __instance.m_nview.GetZDO().SetOwner(uid);
            __instance.m_nview.InvokeRPC(uid, "OpenRespons", true);

            return false;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.TakeAll)), HarmonyPrefix]
        public static bool TakeAllPatch(ref bool __result, Container __instance, Humanoid character) {
            if (__instance.IsOwner()) {
                Log.LogDebug("TakeAll self");
                return true;
            }

            __result = false;

            if (__instance.m_checkGuardStone && !PrivateArea.CheckAccess(__instance.transform.position)) {
                return false;
            }

            long playerID = Game.instance.GetPlayerProfile().GetPlayerID();

            if (!__instance.CheckAccess(playerID)) {
                character.Message(MessageHud.MessageType.Center, "$msg_cantopen");
                return false;
            }

            ContainerHandler.TakeAll(__instance);

            return false;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.UpdateUseVisual)), HarmonyPrefix]
        public static bool ContainerUpdateUseVisualPatch(Container __instance) {
            if (!__instance.m_nview.IsValid() || !Player.m_localPlayer || !InventoryGui.instance) {
                return false;
            }

            bool inUse = __instance == InventoryGui.instance.m_currentContainer || IsContainerInUse(__instance);

            if (__instance.m_open) {
                __instance.m_open.SetActive(inUse);
            }

            if (__instance.m_closed) {
                __instance.m_closed.SetActive(!inUse);
            }

            return false;
        }

        private static bool IsContainerInUse(Container container) {
            ZDOID myId = container.m_nview.GetZDO().m_uid;
            return Player.m_players.Any(p => p != Player.m_localPlayer && p.m_nview.IsValid() && p.m_nview.GetZDO().GetZDOID("accessed-container") == myId);
        }
    }
}
