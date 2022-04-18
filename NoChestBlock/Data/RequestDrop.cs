namespace NoChestBlock {
    public class RequestDrop : IPackage {
        public readonly Vector2i targetContainerSlot;
        public readonly int amount;
        public readonly ZDOID sender;

        public RequestDrop(Vector2i targetContainerSlot, int amount, ZDOID sender) {
            this.targetContainerSlot = targetContainerSlot;
            this.amount = amount;
            this.sender = sender;
        }

        public RequestDrop(ZPackage package) {
            targetContainerSlot = package.ReadVector2i();
            amount = package.ReadInt();
            sender = package.ReadZDOID();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(targetContainerSlot);
            package.Write(amount);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
#if FULL_DEBUG

            Log.LogDebug($"RequestDrop:");
            Log.LogDebug($"  targetContainerSlot: {targetContainerSlot}");
            Log.LogDebug($"  amount: {amount}");
#endif
        }
    }
}
