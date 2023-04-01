using System;
using UnityEngine;

namespace MultiUserChest {
    public class RequestChestAdd : RequestAdd, IRequest {
        public int RequestID { get; set; }
        public Inventory SourceInventory { get; }
        public Inventory TargetInventory { get; }

        public readonly Vector2i toPos;
        public readonly ItemDrop.ItemData dragItem;
        public readonly bool allowSwitch;

        public RequestChestAdd(Vector2i toPos, int dragAmount, ItemDrop.ItemData dragItem, Inventory sourceInventory, Inventory targetInventory) {
            this.toPos = toPos;

            if (dragItem != null) {
                this.dragItem = dragItem.Clone();
                this.dragItem.m_stack = Mathf.Min(dragAmount, this.dragItem.m_stack);
                allowSwitch = dragAmount == dragItem.m_stack;
            }

            SourceInventory = sourceInventory;
            TargetInventory = targetInventory;
        }

        public RequestChestAdd(ZPackage package) {
            RequestID = package.ReadInt();
            toPos = package.ReadVector2i();
            dragItem = InventoryHelper.LoadItemFromPackage(package);
            allowSwitch = package.ReadBool();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(RequestID);
            package.Write(toPos);
            InventoryHelper.WriteItemToPackage(dragItem, package);
            package.Write(allowSwitch);

            return package;
        }

#if DEBUG
        public void PrintDebug() {
            Log.LogDebug($"RequestItemAdd:");
            Log.LogDebug($"  id: {RequestID}");
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
