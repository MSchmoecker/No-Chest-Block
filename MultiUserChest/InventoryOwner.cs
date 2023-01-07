using MultiUserChest.Patches;

namespace MultiUserChest {
    public abstract class InventoryOwner {
        public abstract ZNetView ZNetView { get; }
        public abstract Inventory Inventory { get; }

        public virtual bool IsValid() {
            return ZNetView && Inventory != null;
        }

        public static InventoryOwner GetInventoryObject(Inventory inventory) {
            if (HumanoidExtend.GetHumanoid(inventory, out HumanoidInventoryOwner fromHumanoid)) {
                return fromHumanoid;
            }

            if (ContainerExtend.GetContainer(inventory, out ContainerInventoryOwner fromContainer)) {
                return fromContainer;
            }

            return null;
        }

        public static InventoryOwner GetInventoryObjectOfItem(ItemDrop.ItemData item) {
            if (InventoryPatch.InventoryOfItem.TryGetValue(item, out Inventory fromInventory)) {
                return GetInventoryObject(fromInventory);
            }

            return null;
        }
    }

    public class ContainerInventoryOwner : InventoryOwner {
        public Container Container { get; }

        public ContainerInventoryOwner(Container container) {
            Container = container;
        }

        public override ZNetView ZNetView => Container.m_nview;
        public override Inventory Inventory => Container.GetInventory();
    }

    public class HumanoidInventoryOwner : InventoryOwner {
        public Humanoid Humanoid { get; }

        public HumanoidInventoryOwner(Humanoid humanoid) {
            Humanoid = humanoid;
        }

        public override ZNetView ZNetView => Humanoid.m_nview;
        public override Inventory Inventory => Humanoid.GetInventory();
    }
}
