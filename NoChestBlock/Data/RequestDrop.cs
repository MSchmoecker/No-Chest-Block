namespace NoChestBlock {
    public class RequestDrop : IPackage {
        public readonly Vector2i targetContainerSlot;
        public readonly int amount;

        public RequestDrop(Vector2i targetContainerSlot, int amount) {
            this.targetContainerSlot = targetContainerSlot;
            this.amount = amount;
        }

        public RequestDrop(ZPackage package) {
            targetContainerSlot = package.ReadVector2i();
            amount = package.ReadInt();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(targetContainerSlot);
            package.Write(amount);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestDrop:");
            Log.LogDebug($"  targetContainerSlot: {targetContainerSlot}");
            Log.LogDebug($"  amount: {amount}");
        }
    }
}
