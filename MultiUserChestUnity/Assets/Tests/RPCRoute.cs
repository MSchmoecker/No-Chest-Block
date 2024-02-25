using System;
using System.Reflection;
using HarmonyLib;
using MultiUserChest;
using MultiUserChest.Patches;
using NUnit.Framework;

namespace UnitTests {
    public class RPCRoute {
        private Harmony harmony;
        private static Container container;

        public const string RequestMoveRPC = "MUC_RequestItemMove";
        public const string RequestMoveResponseRPC = "MUC_RequestItemMoveResponse";

        public const string RequestAddRPC = "MUC_RequestItemAdd";
        public const string RequestAddResponseRPC = "MUC_RequestItemAddResponse";

        public const string RequestRemoveRPC = "MUC_RequestItemRemove";
        public const string RequestRemoveResponseRPC = "MUC_RequestItemRemoveResponse";

        public const string RequestConsumeRPC = "MUC_RequestItemConsume";
        public const string RequestConsumeResponseRPC = "MUC_RequestItemConsumeResponse";

        public const string RequestDropRPC = "MUC_RequestItemDrop";
        public const string RequestDropResponseRPC = "MUC_RequestItemDropResponse";

        [SetUp]
        public void SetUp() {
            container = Helper.CreateContainer();

            ZNetSimulate.routedRpcs.Clear();
        }

        [OneTimeSetUp]
        public void OneTimeSetUp() {
            harmony = new Harmony("RPCRoute");
            harmony.PatchAll(typeof(RPCRoutePatches));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() {
            harmony.UnpatchSelf();
        }

        class CallThrow : Exception {
            public CallThrow(string message) : base(message) {
            }
        }

        public static class RPCRoutePatches {
            [HarmonyPatch(typeof(Container), nameof(Container.IsOwner)), HarmonyPrefix]
            public static bool ContainerIsOwnerAlwaysFalsePatch(ref bool __result) {
                __result = false;
                return false;
            }

            [HarmonyPatch(typeof(ZRoutedRpc), nameof(ZRoutedRpc.InvokeRoutedRPC), new[] { typeof(long), typeof(string), typeof(object[]) }), HarmonyPrefix]
            public static void InvokeRPCPatch(string methodName) {
                throw new CallThrow(methodName);
            }
        }

        [Test]
        public void ZRoutedRpcHasRegisterRPCs() {
            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestMoveRPC.GetStableHashCode()));
            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestMoveResponseRPC.GetStableHashCode()));

            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestAddRPC.GetStableHashCode()));
            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestAddResponseRPC.GetStableHashCode()));

            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestRemoveRPC.GetStableHashCode()));
            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestRemoveResponseRPC.GetStableHashCode()));

            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestConsumeRPC.GetStableHashCode()));
            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestConsumeResponseRPC.GetStableHashCode()));

            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestDropRPC.GetStableHashCode()));
            Assert.IsTrue(ZRoutedRpc.instance.m_functions.ContainsKey(RequestDropResponseRPC.GetStableHashCode()));
        }

        private static ZPackage NewSendAbleRPC_ZPackage() {
            ZPackage pgk = new ZPackage();
            pgk.Write(0L);
            pgk.Write(new ZPackage());
            pgk.SetPos(0);
            return pgk;
        }

        private static void TestRPCCallsResponse(string rpc, string rpcResponse, IPackage package) {
            RoutedMethodBase toCall = ZRoutedRpc.instance.m_functions[rpc.GetStableHashCode()];
            var exception = Assert.Throws<TargetInvocationException>(() => {
                ZRoutedRpc.RoutedRPCData routedRpcData = new ZRoutedRpc.RoutedRPCData();
                routedRpcData.m_senderPeerID = 0L; //this.m_id;
                routedRpcData.m_targetPeerID = 0L; //targetPeerID;
                routedRpcData.m_targetZDO = ZDOID.None; //targetZDO;
                routedRpcData.m_methodHash = rpc.GetStableHashCode(); //methodName.GetStableHashCode();
                ZRpc.Serialize(new object[] { ZDOID.None, package.WriteToPackage() }, ref routedRpcData.m_parameters);
                routedRpcData.m_parameters.SetPos(0);
                toCall.Invoke(routedRpcData.m_senderPeerID, routedRpcData.m_parameters);
            });
            Assert.IsInstanceOf<CallThrow>(exception.InnerException);
            Assert.AreEqual(rpcResponse, exception.InnerException.Message);
        }

        [Test]
        public void RequestMoveCallsResponse() {
            TestRPCCallsResponse(RequestMoveRPC, RequestMoveResponseRPC, new RequestMove(null, Vector2i.zero, 0, null));
        }

        [Test]
        public void RequestAddCallsResponse() {
            TestRPCCallsResponse(RequestAddRPC, RequestAddResponseRPC, new RequestChestAdd(Vector2i.zero, 0, null, null, null));
        }

        [Test]
        public void RequestRemoveCallsResponse() {
            TestRPCCallsResponse(RequestRemoveRPC, RequestRemoveResponseRPC, new RequestChestRemove(Vector2i.zero, Vector2i.zero, 0, null, null, null));
        }

        [Test]
        public void RequestConsumeCallsResponse() {
            TestRPCCallsResponse(RequestConsumeRPC, RequestConsumeResponseRPC, new RequestConsume(new ItemDrop.ItemData()));
        }

        [Test]
        public void RequestDropResponse() {
            TestRPCCallsResponse(RequestDropRPC, RequestDropResponseRPC, new RequestDrop(Vector2i.zero, 0, ZDOID.None));
        }
    }
}
