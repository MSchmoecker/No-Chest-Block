namespace MultiUserChest {
    public class RequestAddResponse : IPackage, IResponse {
        public bool Success { get; }
        public int Amount { get; }
        public readonly Vector2i inventoryPos;
        public readonly int inventoryHash;
        public readonly ItemDrop.ItemData switchItem;
        public readonly ZDOID sender;

        public RequestAddResponse(bool success, Vector2i inventoryPos, int amount, int inventoryHash, ItemDrop.ItemData switchItem, ZDOID sender) {
            Success = success;
            Amount = amount;
            this.inventoryPos = inventoryPos;
            this.inventoryHash = inventoryHash;
            this.switchItem = switchItem;
            this.sender = sender;
        }

        public RequestAddResponse(ZPackage package) {
            inventoryPos = package.ReadVector2i();
            Success = package.ReadBool();
            Amount = package.ReadInt();
            inventoryHash = package.ReadInt();
            switchItem = InventoryHelper.LoadItemFromPackage(package);
            sender = package.ReadZDOID();
        }

        public RequestAddResponse() {
            Success = false;
            inventoryPos = new Vector2i(-1, -1);
            Amount = 0;
            inventoryHash = 0;
            switchItem = null;
            sender = ZDOID.None;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(inventoryPos);
            package.Write(Success);
            package.Write(Amount);
            package.Write(inventoryHash);
            InventoryHelper.WriteItemToPackage(switchItem, package);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
#if FULL_DEBUG
            Log.LogDebug($"RequestAddResponse:");
            Log.LogDebug($"  inventoryPos: {inventoryPos}");
            Log.LogDebug($"  success: {Success}");
            Log.LogDebug($"  amount: {Amount}");
            Log.LogDebug($"  inventoryHash: {inventoryHash}");
            Log.LogDebug($"  switchItem: {switchItem != null}");
            Log.LogDebug($"  sender: {sender}");
            InventoryHelper.PrintItem(switchItem);
#endif
        }
    }
}
