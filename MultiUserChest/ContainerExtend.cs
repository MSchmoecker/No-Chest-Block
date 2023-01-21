using System.Runtime.CompilerServices;
using UnityEngine;

namespace MultiUserChest {
    public class ContainerExtend : MonoBehaviour {
        private static readonly ConditionalWeakTable<Inventory, ContainerInventoryOwner> Containers = new ConditionalWeakTable<Inventory, ContainerInventoryOwner>();
        private Container container;

        private void Awake() {
            container = GetComponent<Container>();
            Containers.Add(container.GetInventory(), new ContainerInventoryOwner(container));
        }

        private void OnDestroy() {
            Containers.Remove(container.GetInventory());
        }

        public static bool GetContainer(Inventory inventory, out ContainerInventoryOwner container) {
            return Containers.TryGetValue(inventory, out container);
        }
    }
}
