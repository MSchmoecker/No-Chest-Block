namespace MultiUserChest {
    public class RequestMoveResponse : IPackage, IResponse {
        public int SourceID { get; set; }
        public bool Success { get; set; }
        public int Amount { get; set; }

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

#if DEBUG
        public void PrintDebug() {
            Log.LogDebug($"RequestMoveResponse:");
            Log.LogDebug($"  SourceID: {SourceID}");
            Log.LogDebug($"  Success: {Success}");
            Log.LogDebug($"  Amount: {Amount}");
        }
#endif
    }
}
