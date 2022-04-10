namespace NoChestBlock {
    public class RequestAdd : IPackage {
        public readonly Vector2i toPos;
        public readonly int dragAmount;
        public readonly ItemDrop.ItemData dragItem;
        public readonly int fromInventoryHash;
        public readonly ZDOID sender;
        public readonly bool allowSwitch;

        public RequestAdd(Vector2i toPos, int dragAmount, ItemDrop.ItemData dragItem, string inventoryName, bool allowSwitch, ZDOID sender) {
            this.toPos = toPos;
            this.dragAmount = dragAmount;
            this.dragItem = dragItem;
            fromInventoryHash = inventoryName.GetStableHashCode();
            this.allowSwitch = allowSwitch;
            this.sender = sender;
        }

        public RequestAdd(ZPackage package) {
            toPos = package.ReadVector2i();
            dragAmount = package.ReadInt();
            fromInventoryHash = package.ReadInt();
            dragItem = InventoryHelper.LoadItemFromPackage(package);
            allowSwitch = package.ReadBool();
            sender = package.ReadZDOID();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(toPos);
            package.Write(dragAmount);
            package.Write(fromInventoryHash);
            InventoryHelper.WriteItemToPackage(dragItem, package);
            package.Write(allowSwitch);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestItemAdd:");
            Log.LogDebug($"  toContainer: {toPos}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
            Log.LogDebug($"  inventoryHashFrom: {fromInventoryHash}");
            Log.LogDebug($"  allowSwitch: {allowSwitch}");
            Log.LogDebug($"  dragItem: {dragItem != null}");
            Log.LogDebug($"  sender: {sender}");
            InventoryHelper.PrintItem(dragItem);
        }
    }
}
