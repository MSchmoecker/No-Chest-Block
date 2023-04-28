namespace MultiUserChest {
    public class RequestMoveResponse : IPackage, IResponse {
        public int SourceID { get; }
        public bool Success { get; }
        public int Amount { get; }

        public RequestMoveResponse(int sourceID, bool success, int amount) {
            SourceID = sourceID;
            Success = success;
            Amount = amount;
        }

        public RequestMoveResponse() {
            SourceID = 0;
            Success = false;
            Amount = 0;
        }

        public RequestMoveResponse(ZPackage package) {
            SourceID = package.ReadInt();
            Success = package.ReadBool();
            Amount = package.ReadInt();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(SourceID);
            package.Write(Success);
            package.Write(Amount);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestMoveResponse:");
            Log.LogDebug($"  SourceID: {SourceID}");
            Log.LogDebug($"  Success: {Success}");
            Log.LogDebug($"  Amount: {Amount}");
        }
    }
}
