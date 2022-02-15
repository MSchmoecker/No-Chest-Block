namespace NoChestBlock {
    public class RequestDrop : IPackage {
        public Vector2i targetContainerSlot;
        public int amount;

        public RequestDrop(Vector2i targetContainerSlot, int amount) {
            this.targetContainerSlot = targetContainerSlot;
            this.amount = amount;
        }

        public RequestDrop(ZPackage package) {
            ReadFromPackage(package);
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(targetContainerSlot);
            package.Write(amount);

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            targetContainerSlot = package.ReadVector2i();
            amount = package.ReadInt();
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestDrop:");
            Log.LogInfo($"  targetContainerSlot: {targetContainerSlot}");
            Log.LogInfo($"  amount: {amount}");
        }
    }
}
