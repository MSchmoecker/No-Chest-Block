using System;

namespace MultiUserChest {
    public class RequestChestRemove : IRequest {
        public int RequestID { get; set; }
        public Inventory SourceInventory { get; }
        public Inventory TargetInventory { get; }

        public readonly Vector2i fromPos;
        public readonly Vector2i toPos;
        public readonly int dragAmount;
        public readonly ItemDrop.ItemData switchItem;
        public readonly ItemDrop.ItemData item;

        public RequestChestRemove(Vector2i fromPos, Vector2i toPos, int dragAmount, ItemDrop.ItemData switchItem, Inventory sourceInventory, Inventory targetInventory) {
            this.fromPos = fromPos;
            this.toPos = toPos;
            this.dragAmount = dragAmount;
            this.switchItem = switchItem?.Clone();
            SourceInventory = sourceInventory;
            TargetInventory = targetInventory;
            item = sourceInventory.GetItemAt(fromPos.x, fromPos.y)?.Clone();
        }

        public RequestChestRemove(ZPackage package) {
            RequestID = package.ReadInt();
            fromPos = package.ReadVector2i();
            toPos = package.ReadVector2i();
            dragAmount = package.ReadInt();
            switchItem = InventoryHelper.LoadItemFromPackage(package);
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(RequestID);
            package.Write(fromPos);
            package.Write(toPos);
            package.Write(dragAmount);
            InventoryHelper.WriteItemToPackage(switchItem, package);

            return package;
        }

#if DEBUG
        public void PrintDebug() {
            Log.LogDebug($"RequestItemRemove:");
            Log.LogDebug($"  id: {RequestID}");
            Log.LogDebug($"  fromContainer: {fromPos}");
            Log.LogDebug($"  toInventory: {toPos}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
            InventoryHelper.PrintItem(nameof(switchItem), switchItem);
        }
#endif
    }
}
