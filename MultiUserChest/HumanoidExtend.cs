using System.Runtime.CompilerServices;
using UnityEngine;

namespace MultiUserChest {
    public class HumanoidExtend : MonoBehaviour {
        private static readonly ConditionalWeakTable<Inventory, HumanoidInventoryOwner> Humanoids = new ConditionalWeakTable<Inventory, HumanoidInventoryOwner>();
        private Humanoid humanoid;

        private void Awake() {
            humanoid = GetComponent<Humanoid>();
            Humanoids.Add(humanoid.GetInventory(), new HumanoidInventoryOwner(humanoid));
        }

        private void OnDestroy() {
            Humanoids.Remove(humanoid.GetInventory());
        }

        public static bool GetHumanoid(Inventory inventory, out HumanoidInventoryOwner humanoid) {
            return Humanoids.TryGetValue(inventory, out humanoid);
        }
    }
}
