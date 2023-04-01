using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MultiUserChest {
    public class HumanoidExtend : MonoBehaviour {
        private static readonly ConditionalWeakTable<Inventory, HumanoidInventoryOwner> Humanoids = new ConditionalWeakTable<Inventory, HumanoidInventoryOwner>();
        private Inventory inventory;

        private void Awake() {
            UpdateInventory();
        }

        private void Start() {
            UpdateInventory();
        }

        private void UpdateInventory() {
            if (!TryGetComponent(out Humanoid humanoid)) {
                return;
            }

            inventory = humanoid.GetInventory();

            if (inventory == null) {
                return;
            }

            foreach (Inventory subInventory in inventory.GetInventories()) {
                Humanoids.TryAdd(subInventory, i => new HumanoidInventoryOwner(humanoid, i));
            }
        }

        private void OnDestroy() {
            if (inventory == null) {
                return;
            }

            foreach (Inventory subInventory in inventory.GetInventories()) {
                Humanoids.Remove(subInventory);
            }
        }

        public static bool GetHumanoid(Inventory inventory, out HumanoidInventoryOwner humanoid) {
            return Humanoids.TryGetValue(inventory, out humanoid);
        }
    }
}
