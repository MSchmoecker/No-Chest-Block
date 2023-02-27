using MultiUserChest.Patches;

namespace MultiUserChest {
    public abstract class InventoryOwner {
        public abstract ZNetView ZNetView { get; }
        public abstract Inventory Inventory { get; }

        public virtual bool IsValid() {
            return ZNetView && ZNetView.HasOwner() && Inventory != null;
        }

        public static InventoryOwner GetOwner(Inventory inventory) {
            if (HumanoidExtend.GetHumanoid(inventory, out HumanoidInventoryOwner fromHumanoid)) {
                return fromHumanoid;
            }

            if (ContainerExtend.GetContainer(inventory, out ContainerInventoryOwner fromContainer)) {
                return fromContainer;
            }

            return null;
        }

        public static InventoryOwner GetOwner(ItemDrop.ItemData item) {
            if (InventoryPatch.InventoryOfItem.TryGetValue(item, out Inventory fromInventory)) {
                return GetOwner(fromInventory);
            }

            return null;
        }

        public string GetDescription() {
            if (!IsValid()) {
                return "-";
            }

            return $"{Inventory.GetName()} ({GetType().Name}), is owner: { ZNetView.IsOwner()}";
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
        private readonly Inventory inventory;

        public HumanoidInventoryOwner(Humanoid humanoid, Inventory inventory) {
            Humanoid = humanoid;
            this.inventory = inventory;
        }

        public override ZNetView ZNetView => Humanoid.m_nview;
        public override Inventory Inventory => inventory;
    }
}
