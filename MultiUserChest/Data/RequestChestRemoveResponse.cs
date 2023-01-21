namespace MultiUserChest {
    public class RequestChestRemoveResponse : IPackage, IResponse {
        public bool Success { get; }
        public int Amount { get; }
        public readonly Vector2i inventoryPos;
        public readonly int inventoryHash;
        public readonly ItemDrop.ItemData responseItem;
        public readonly ZDOID sender;
        public readonly bool hasSwitched;

        public RequestChestRemoveResponse(bool success, int amount, bool hasSwitched, Vector2i inventoryPos, int inventoryHash, ItemDrop.ItemData responseItem, ZDOID sender) {
            Success = success;
            Amount = amount;
            this.hasSwitched = hasSwitched;
            this.inventoryPos = inventoryPos;
            this.inventoryHash = inventoryHash;
            this.responseItem = responseItem;
            this.sender = sender;
        }

        public RequestChestRemoveResponse(ZPackage package) {
            Success = package.ReadBool();
            Amount = package.ReadInt();
            hasSwitched = package.ReadBool();
            inventoryPos = package.ReadVector2i();
            inventoryHash = package.ReadInt();
            responseItem = InventoryHelper.LoadItemFromPackage(package);
            sender = package.ReadZDOID();
        }

        public RequestChestRemoveResponse() {
            Success = false;
            Amount = 0;
            hasSwitched = false;
            inventoryPos = new Vector2i(-1, -1);
            inventoryHash = 0;
            responseItem = null;
            sender = ZDOID.None;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(Success);
            package.Write(Amount);
            package.Write(hasSwitched);
            package.Write(inventoryPos);
            package.Write(inventoryHash);
            InventoryHelper.WriteItemToPackage(responseItem, package);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
#if FULL_DEBUG
            Log.LogDebug($"RequestRemoveResponse:");
            Log.LogDebug($"  success: {Success}");
            Log.LogDebug($"  amount: {Amount}");
            Log.LogDebug($"  hasSwitched: {hasSwitched}");
            Log.LogDebug($"  inventoryPos: {inventoryPos}");
            Log.LogDebug($"  inventoryName: {inventoryHash}");
            Log.LogDebug($"  responseItem: {responseItem != null}");
            Log.LogDebug($"  sender: {sender}");
            InventoryHelper.PrintItem(responseItem);
#endif
        }
    }
}
