namespace NoChestBlock {
    public class RequestMove : IPackage {
        public Vector2i fromPos;
        public Vector2i toPos;
        public int dragAmount;

        public RequestMove(Vector2i fromPos, Vector2i toPos, int dragAmount) {
            this.fromPos = fromPos;
            this.toPos = toPos;
            this.dragAmount = dragAmount;
        }

        public RequestMove(ZPackage package) {
            ReadFromPackage(package);
        }

        public RequestMove() {
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(fromPos);
            package.Write(toPos);
            package.Write(dragAmount);

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            fromPos = package.ReadVector2i();
            toPos = package.ReadVector2i();
            dragAmount = package.ReadInt();
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestMove:");
            Log.LogDebug($"  fromPos: {fromPos}");
            Log.LogDebug($"  toPos: {toPos}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
        }
    }
}
