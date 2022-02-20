namespace NoChestBlock {
    public class RequestMove : IPackage {
        public readonly Vector2i fromPos;
        public readonly Vector2i toPos;
        public readonly int dragAmount;

        public RequestMove(Vector2i fromPos, Vector2i toPos, int dragAmount) {
            this.fromPos = fromPos;
            this.toPos = toPos;
            this.dragAmount = dragAmount;
        }

        public RequestMove(ZPackage package) {
            fromPos = package.ReadVector2i();
            toPos = package.ReadVector2i();
            dragAmount = package.ReadInt();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(fromPos);
            package.Write(toPos);
            package.Write(dragAmount);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestMove:");
            Log.LogDebug($"  fromPos: {fromPos}");
            Log.LogDebug($"  toPos: {toPos}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
        }
    }
}
