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

        private const string RequestMoveRPC = "MUC_RequestItemMove";
        private const string RequestMoveResponseRPC = "MUC_RequestItemMoveResponse";

        private const string RequestAddRPC = "MUC_RequestItemAdd";
        private const string RequestAddResponseRPC = "MUC_RequestItemAddResponse";

        private const string RequestRemoveRPC = "MUC_RequestItemRemove";
        private const string RequestRemoveResponseRPC = "MUC_RequestItemRemoveResponse";

        private const string RequestConsumeRPC = "MUC_RequestItemConsume";
        private const string RequestConsumeResponseRPC = "MUC_RequestItemConsumeResponse";

        private const string RequestDropRPC = "MUC_RequestItemDrop";
        private const string RequestDropResponseRPC = "MUC_RequestItemDropResponse";

        [SetUp]
        public void SetUp() {
            container = Helper.CreateContainer();
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

            [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.InvokeRPC), new[] { typeof(long), typeof(string), typeof(object[]) }), HarmonyPrefix]
            public static void InvokeRPCPatch(string method) {
                throw new CallThrow(method);
            }
        }

        [Test]
        public void ContainerHasRegisterRPCs() {
            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestMoveRPC.GetStableHashCode()));
            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestMoveResponseRPC.GetStableHashCode()));

            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestAddRPC.GetStableHashCode()));
            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestAddResponseRPC.GetStableHashCode()));

            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestRemoveRPC.GetStableHashCode()));
            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestRemoveResponseRPC.GetStableHashCode()));

            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestConsumeRPC.GetStableHashCode()));
            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestConsumeResponseRPC.GetStableHashCode()));

            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestDropRPC.GetStableHashCode()));
            Assert.IsTrue(container.m_nview.m_functions.ContainsKey(RequestDropResponseRPC.GetStableHashCode()));
        }

        private static ZPackage NewSendAbleRPC_ZPackage() {
            ZPackage pgk = new ZPackage();
            pgk.Write(0L);
            pgk.Write(new ZPackage());
            pgk.SetPos(0);
            return pgk;
        }

        private static ZPackage NewSendAbleRPC_Bool() {
            ZPackage pgk = new ZPackage();
            pgk.Write(0L);
            pgk.Write(false);
            pgk.SetPos(0);
            return pgk;
        }

        private static void TestRPCCallsResponse(string rpc, string rpcResponse, ZPackage package) {
            RoutedMethodBase toCall = container.m_nview.m_functions[rpc.GetStableHashCode()];
            var exception = Assert.Throws<TargetInvocationException>(() => toCall.Invoke(0, package));
            Assert.IsInstanceOf<CallThrow>(exception.InnerException);
            Assert.AreEqual(rpcResponse, exception.InnerException.Message);
        }

        [Test]
        public void RequestMoveCallsResponse() {
            TestRPCCallsResponse(RequestMoveRPC, RequestMoveResponseRPC, NewSendAbleRPC_ZPackage());
        }

        [Test]
        public void RequestAddCallsResponse() {
            TestRPCCallsResponse(RequestAddRPC, RequestAddResponseRPC, NewSendAbleRPC_ZPackage());
        }

        [Test]
        public void RequestRemoveCallsResponse() {
            TestRPCCallsResponse(RequestRemoveRPC, RequestRemoveResponseRPC, NewSendAbleRPC_ZPackage());
        }

        [Test]
        public void RequestConsumeCallsResponse() {
            TestRPCCallsResponse(RequestConsumeRPC, RequestConsumeResponseRPC, NewSendAbleRPC_ZPackage());
        }

        [Test]
        public void RequestDropResponse() {
            TestRPCCallsResponse(RequestDropRPC, RequestDropResponseRPC, NewSendAbleRPC_ZPackage());
        }
    }
}
