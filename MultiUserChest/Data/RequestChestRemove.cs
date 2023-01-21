namespace MultiUserChest {
    public class RequestChestRemove : IPackage {
        public readonly Vector2i fromPos;
        public readonly Vector2i toPos;
        public readonly int dragAmount;
        public readonly ItemDrop.ItemData switchItem;
        public readonly int fromInventoryHash;
        public readonly ZDOID sender;

        public RequestChestRemove(Vector2i fromPos, Vector2i toPos, int dragAmount, string fromInventory, ItemDrop.ItemData switchItem, ZDOID sender) {
            this.fromPos = fromPos;
            this.toPos = toPos;
            this.dragAmount = dragAmount;
            fromInventoryHash = fromInventory.GetStableHashCode();
            this.switchItem = switchItem?.Clone();
            this.sender = sender;
        }

        public RequestChestRemove(ZPackage package) {
            fromPos = package.ReadVector2i();
            toPos = package.ReadVector2i();
            dragAmount = package.ReadInt();
            fromInventoryHash = package.ReadInt();
            switchItem = InventoryHelper.LoadItemFromPackage(package);
            sender = package.ReadZDOID();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(fromPos);
            package.Write(toPos);
            package.Write(dragAmount);
            package.Write(fromInventoryHash);
            InventoryHelper.WriteItemToPackage(switchItem, package);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
#if FULL_DEBUG
            Log.LogDebug($"RequestItemRemove:");
            Log.LogDebug($"  fromContainer: {fromPos}");
            Log.LogDebug($"  toInventory: {toPos}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
            Log.LogDebug($"  inventoryHashFrom: {fromInventoryHash}");
            Log.LogDebug($"  sender: {sender}");
            InventoryHelper.PrintItem(nameof(switchItem), switchItem);
#endif
        }
    }
}