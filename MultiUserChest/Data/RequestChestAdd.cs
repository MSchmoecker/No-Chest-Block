using UnityEngine;

namespace MultiUserChest {
    public class RequestChestAdd : IPackage {
        public readonly Vector2i toPos;
        public readonly ItemDrop.ItemData dragItem;
        public readonly int fromInventoryHash;
        public readonly ZDOID sender;
        public readonly bool allowSwitch;

        public RequestChestAdd(Vector2i toPos, int dragAmount, ItemDrop.ItemData dragItem, string inventoryName, ZDOID sender) {
            this.toPos = toPos;

            if (dragItem != null) {
                this.dragItem = dragItem.Clone();
                this.dragItem.m_stack = Mathf.Min(dragAmount, this.dragItem.m_stack);
                allowSwitch = dragAmount == dragItem.m_stack;
            }

            fromInventoryHash = inventoryName.GetStableHashCode();
            this.sender = sender;
        }

        public RequestChestAdd(ZPackage package) {
            toPos = package.ReadVector2i();
            fromInventoryHash = package.ReadInt();
            dragItem = InventoryHelper.LoadItemFromPackage(package);
            allowSwitch = package.ReadBool();
            sender = package.ReadZDOID();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(toPos);
            package.Write(fromInventoryHash);
            InventoryHelper.WriteItemToPackage(dragItem, package);
            package.Write(allowSwitch);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
#if FULL_DEBUG
            Log.LogDebug($"RequestItemAdd:");
            Log.LogDebug($"  toContainer: {toPos}");
            Log.LogDebug($"  inventoryHashFrom: {fromInventoryHash}");
            Log.LogDebug($"  allowSwitch: {allowSwitch}");
            Log.LogDebug($"  sender: {sender}");
            InventoryHelper.PrintItem(nameof(dragItem), dragItem);
#endif
        }
    }
}
