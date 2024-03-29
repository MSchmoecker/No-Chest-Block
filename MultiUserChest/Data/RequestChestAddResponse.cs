namespace MultiUserChest {
    public class RequestChestAddResponse : IPackage, IResponse {
        public int SourceID { get; set; }
        public bool Success { get; set; }
        public int Amount { get; set; }

        public Vector2i inventoryPos;
        public ItemDrop.ItemData switchItem;

        public RequestChestAddResponse(int sourceID, bool success, Vector2i inventoryPos, int amount, ItemDrop.ItemData switchItem) {
            SourceID = sourceID;
            Success = success;
            Amount = amount;
            this.inventoryPos = inventoryPos;
            this.switchItem = switchItem.ClampStackSize();
        }

        public RequestChestAddResponse(ZPackage package) {
            SourceID = package.ReadInt();
            inventoryPos = package.ReadVector2i();
            Success = package.ReadBool();
            Amount = package.ReadInt();
            switchItem = InventoryHelper.LoadItemFromPackage(package);
        }

        public RequestChestAddResponse() {
            SourceID = 0;
            Success = false;
            inventoryPos = new Vector2i(-1, -1);
            Amount = 0;
            switchItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(SourceID);
            package.Write(inventoryPos);
            package.Write(Success);
            package.Write(Amount);
            InventoryHelper.WriteItemToPackage(switchItem, package);

            return package;
        }

#if DEBUG
        public void PrintDebug() {
            Log.LogDebug($"RequestChestAddResponse:");
            Log.LogDebug($"  id: {SourceID}");
            Log.LogDebug($"  inventoryPos: {inventoryPos}");
            Log.LogDebug($"  success: {Success}");
            Log.LogDebug($"  amount: {Amount}");
            InventoryHelper.PrintItem(nameof(switchItem), switchItem);
        }
#endif
    }
}
