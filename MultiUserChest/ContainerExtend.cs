using System.Runtime.CompilerServices;
using UnityEngine;

namespace MultiUserChest {
    public class ContainerExtend : MonoBehaviour {
        private static readonly ConditionalWeakTable<Inventory, ContainerInventoryOwner> Containers = new ConditionalWeakTable<Inventory, ContainerInventoryOwner>();
        private Inventory inventory;

        private void Awake() {
            if (!TryGetComponent(out Container container)) {
                return;
            }

            inventory = container.GetInventory();

            if (inventory == null) {
                return;
            }

            Containers.TryAdd(inventory, i => new ContainerInventoryOwner(container));
        }

        private void OnDestroy() {
            if (inventory != null) {
                Containers.Remove(inventory);
            }
        }

        public static bool GetContainer(Inventory inventory, out ContainerInventoryOwner container) {
            return Containers.TryGetValue(inventory, out container);
        }
    }
}
