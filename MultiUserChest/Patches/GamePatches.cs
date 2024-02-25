using HarmonyLib;

namespace MultiUserChest.Patches {
    [HarmonyPatch]
    public static class GamePatches {
        [HarmonyPatch(typeof(Game), nameof(Game.Start)), HarmonyPostfix]
        public static void GameStartPatch() {
            RegisterRPCs();
        }

        public static void RegisterRPCs() {
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemMoveRPC, ContainerRPCHandler.RPC_RequestItemMove);
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemAddRPC, ContainerRPCHandler.RPC_RequestItemAdd);
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemRemoveRPC, ContainerRPCHandler.RPC_RequestItemRemove);
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemConsumeRPC, ContainerRPCHandler.RPC_RequestItemConsume);
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemDropRPC, ContainerRPCHandler.RPC_RequestDrop);

            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemMoveResponseRPC, InventoryHandler.RPC_RequestItemMoveResponse);
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemAddResponseRPC, InventoryHandler.RPC_RequestItemAddResponse);
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemRemoveResponseRPC, InventoryHandler.RPC_RequestItemRemoveResponse);
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemConsumeResponseRPC, InventoryHandler.RPC_RequestItemConsumeResponse);
            ZRoutedRpc.instance.Register<ZDOID, ZPackage>(ContainerPatch.ItemDropResponseRPC, InventoryHandler.RPC_RequestDropResponse);
        }

        public static void InvokeRPC(ZNetView netView, string rpc, IPackage package) {
#if DEBUG
            Timer.Start(package);
#endif

            ZRoutedRpc.instance.InvokeRoutedRPC(netView.m_zdo.GetOwner(), rpc, netView.m_zdo.m_uid, package.WriteToPackage());
        }
    }
}
