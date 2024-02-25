using System;
using System.Collections.Generic;
using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    public class ZNetSimulate {
        public static Queue<RoutedNetViewRpc> routedRpcs = new Queue<RoutedNetViewRpc>();

        public class RoutedNetViewRpc {
            public ZNetView netView;
            public string method;
            public object[] parameters;

            public ZRoutedRpc.RoutedRPCData ToRoutedRpcData() {
                ZRoutedRpc.RoutedRPCData routedRpcData = new ZRoutedRpc.RoutedRPCData() {
                    m_methodHash = method.GetStableHashCode(),
                    m_targetZDO = netView ? netView.m_zdo.m_uid : ZDOID.None,
                    m_senderPeerID = ZDOMan.instance.m_sessionID,
                };

                ZRpc.Serialize(parameters, ref routedRpcData.m_parameters);
                routedRpcData.m_parameters.SetPos(0);

                return routedRpcData;
            }
        }

        public static void HandleAllRoutedRpcs() {
            while (routedRpcs.Count > 0) {
                HandleRoutedRpcs(1);
            }
        }

        public static void HandleRoutedRpcs(int count) {
            for (int i = 0; i < count; i++) {
                RoutedNetViewRpc rpc = routedRpcs.Dequeue();

                if (rpc.netView) {
                    rpc.netView.HandleRoutedRPC(rpc.ToRoutedRpcData());
                } else {
                    ZRoutedRpc.instance.HandleRoutedRPC(rpc.ToRoutedRpcData());
                }
            }
        }

        public static TPackage GetRoutedRpc<TPackage>(string expectedMethod, Func<ZPackage, TPackage> converter) {
            RoutedNetViewRpc rpc = routedRpcs.ToArray()[0];
            Assert.AreEqual(expectedMethod, rpc.method);
            Assert.AreEqual(typeof(ZDOID), rpc.parameters[0].GetType());
            Assert.AreEqual(typeof(ZPackage), rpc.parameters[1].GetType());

            ZPackage package = (ZPackage)rpc.parameters[1];
            package.SetPos(0);
            return converter(package);
        }
    }
}
