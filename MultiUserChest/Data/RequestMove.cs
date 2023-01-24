namespace MultiUserChest {
    public class RequestMove : IPackage {
        public readonly Vector2i fromPos;
        public readonly Vector2i toPos;
        public readonly int itemHash;
        public readonly int dragAmount;

        public RequestMove(ItemDrop.ItemData itemToMove, Vector2i toPos, int dragAmount) {
            if (itemToMove == null) {
                Log.LogWarning("Item is null");

                fromPos = new Vector2i(-1, -1);
                this.toPos = new Vector2i(-1, -1);
                itemHash = 0;
                this.dragAmount = 0;
                return;
            }

            fromPos = itemToMove.m_gridPos;
            this.toPos = toPos;
            itemHash = itemToMove.PrefabName().GetStableHashCode();
            this.dragAmount = dragAmount;
        }

        public RequestMove(ZPackage package) {
            fromPos = package.ReadVector2i();
            toPos = package.ReadVector2i();
            itemHash = package.ReadInt();
            dragAmount = package.ReadInt();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(fromPos);
            package.Write(toPos);
            package.Write(itemHash);
            package.Write(dragAmount);

            return package;
        }

        public void PrintDebug() {
#if FULL_DEBUG
            Log.LogDebug($"RequestMove:");
            Log.LogDebug($"  fromPos: {fromPos}");
            Log.LogDebug($"  toPos: {toPos}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
#endif
        }
    }
}
