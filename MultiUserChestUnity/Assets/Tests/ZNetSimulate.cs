using System.Collections.Generic;

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
                    m_targetZDO = netView.m_zdo.m_uid,
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
                rpc.netView.HandleRoutedRPC(rpc.ToRoutedRpcData());
            }
        }
    }
}
