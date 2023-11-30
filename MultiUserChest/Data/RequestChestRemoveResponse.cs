namespace MultiUserChest {
    public class RequestChestRemoveResponse : IPackage, IResponse {
        public int SourceID { get; }

        public bool Success { get; }
        public int Amount { get; }
        public readonly ItemDrop.ItemData responseItem;
        public readonly bool hasSwitched;

        public RequestChestRemoveResponse(int sourceID, bool success, int amount, bool hasSwitched, ItemDrop.ItemData responseItem) {
            SourceID = sourceID;
            Success = success;
            Amount = amount;
            this.hasSwitched = hasSwitched;
            this.responseItem = responseItem.ClampStackSize();
        }

        public RequestChestRemoveResponse(ZPackage package) {
            SourceID = package.ReadInt();
            Success = package.ReadBool();
            Amount = package.ReadInt();
            hasSwitched = package.ReadBool();
            responseItem = InventoryHelper.LoadItemFromPackage(package);
        }

        public RequestChestRemoveResponse() {
            Success = false;
            Amount = 0;
            hasSwitched = false;
            responseItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(SourceID);
            package.Write(Success);
            package.Write(Amount);
            package.Write(hasSwitched);
            InventoryHelper.WriteItemToPackage(responseItem, package);

            return package;
        }

#if DEBUG
        public void PrintDebug() {
            Log.LogDebug($"RequestRemoveResponse:");
            Log.LogDebug($"  success: {Success}");
            Log.LogDebug($"  amount: {Amount}");
            Log.LogDebug($"  hasSwitched: {hasSwitched}");
            InventoryHelper.PrintItem(nameof(responseItem), responseItem);
        }
#endif
    }
}
