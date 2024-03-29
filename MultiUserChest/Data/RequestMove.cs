namespace MultiUserChest {
    public class RequestMove : IRequest {
        public int RequestID { get; set; }
        public Inventory SourceInventory { get; }
        public Inventory TargetInventory { get; }

        public readonly Vector2i fromPos;
        public readonly Vector2i toPos;
        public readonly int itemHash;
        public readonly int dragAmount;
        public readonly ItemDrop.ItemData item;

        public RequestMove(ItemDrop.ItemData itemToMove, Vector2i toPos, int dragAmount, Inventory inventory) {
            if (itemToMove == null) {
                Log.LogWarning("Item is null");

                fromPos = new Vector2i(-1, -1);
                this.toPos = new Vector2i(-1, -1);
                itemHash = 0;
                this.dragAmount = 0;
                return;
            }

            SourceInventory = inventory;
            TargetInventory = inventory;
            fromPos = itemToMove.m_gridPos;
            this.toPos = toPos;
            itemHash = itemToMove.PrefabName().GetStableHashCode();
            item = itemToMove.Clone();
            this.dragAmount = dragAmount;
        }

        public RequestMove(ZPackage package) {
            RequestID = package.ReadInt();
            fromPos = package.ReadVector2i();
            toPos = package.ReadVector2i();
            itemHash = package.ReadInt();
            dragAmount = package.ReadInt();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(RequestID);
            package.Write(fromPos);
            package.Write(toPos);
            package.Write(itemHash);
            package.Write(dragAmount);

            return package;
        }

#if DEBUG
        public void PrintDebug() {
            Log.LogDebug($"RequestMove:");
            Log.LogDebug($"  RequestID: {RequestID}");
            Log.LogDebug($"  fromPos: {fromPos}");
            Log.LogDebug($"  toPos: {toPos}");
            Log.LogDebug($"  itemHash: {itemHash}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
        }
#endif
    }
}
