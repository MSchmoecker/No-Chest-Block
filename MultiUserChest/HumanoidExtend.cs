using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MultiUserChest {
    public class HumanoidExtend : MonoBehaviour {
        private static readonly ConditionalWeakTable<Inventory, HumanoidInventoryOwner> Humanoids = new ConditionalWeakTable<Inventory, HumanoidInventoryOwner>();
        private Humanoid humanoid;

        private void Awake() {
            UpdateInventory();
        }

        private void Start() {
            UpdateInventory();
        }

        private void UpdateInventory() {
            humanoid = GetComponent<Humanoid>();

            if (humanoid.GetInventory().IsExtendedInventory(out List<Inventory> inventories)) {
                foreach (Inventory inventory in inventories) {
                    Humanoids.Remove(inventory);
                    Humanoids.Add(inventory, new HumanoidInventoryOwner(humanoid, inventory));
                }
            } else {
                Humanoids.Remove(humanoid.GetInventory());
                Humanoids.Add(humanoid.GetInventory(), new HumanoidInventoryOwner(humanoid, humanoid.GetInventory()));
            }
        }

        private void OnDestroy() {
            Humanoids.Remove(humanoid.GetInventory());
        }

        public static bool GetHumanoid(Inventory inventory, out HumanoidInventoryOwner humanoid) {
            return Humanoids.TryGetValue(inventory, out humanoid);
        }
    }
}
