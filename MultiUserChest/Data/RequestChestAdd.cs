using System;
using UnityEngine;

namespace MultiUserChest {
    public class RequestChestAdd : RequestAdd, IPackage {
        public int ID { get; }
        public readonly Inventory sourceInventory;
        public readonly Inventory targetInventory;

        public readonly Vector2i toPos;
        public readonly ItemDrop.ItemData dragItem;
        public readonly bool allowSwitch;

        private RequestChestAdd() {
            ID = PackageHandler.AddPackage(this);
        }

        public RequestChestAdd(Vector2i toPos, int dragAmount, ItemDrop.ItemData dragItem, Inventory sourceInventory, Inventory targetInventory) : this() {
            this.toPos = toPos;

            if (dragItem != null) {
                this.dragItem = dragItem.Clone();
                this.dragItem.m_stack = Mathf.Min(dragAmount, this.dragItem.m_stack);
                allowSwitch = dragAmount == dragItem.m_stack;
            }

            this.sourceInventory = sourceInventory;
            this.targetInventory = targetInventory;
        }

        public RequestChestAdd(ZPackage package) : this() {
            ID = package.ReadInt();
            toPos = package.ReadVector2i();
            dragItem = InventoryHelper.LoadItemFromPackage(package);
            allowSwitch = package.ReadBool();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(ID);
            package.Write(toPos);
            InventoryHelper.WriteItemToPackage(dragItem, package);
            package.Write(allowSwitch);

            return package;
        }

#if DEBUG
        public void PrintDebug() {
            Log.LogDebug($"RequestItemAdd:");
            Log.LogDebug($"  id: {ID}");
            Log.LogDebug($"  toContainer: {toPos}");
            Log.LogDebug($"  allowSwitch: {allowSwitch}");
            InventoryHelper.PrintItem(nameof(dragItem), dragItem);
        }
#endif
    }

    [Obsolete]
    public class RequestAdd {
    }
}
