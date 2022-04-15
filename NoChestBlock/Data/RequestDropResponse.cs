﻿namespace NoChestBlock {
    public class RequestDropResponse : IPackage, IResponse {
        public readonly ItemDrop.ItemData responseItem;
        public readonly ZDOID sender;
        public bool Success { get; }
        public int Amount { get; }

        public RequestDropResponse(ItemDrop.ItemData responseItem, ZDOID sender, bool success, int amount) {
            this.responseItem = responseItem;
            this.sender = sender;
            Success = success;
            Amount = amount;
        }

        public RequestDropResponse(ZPackage package) {
            responseItem = InventoryHelper.LoadItemFromPackage(package);
            sender = package.ReadZDOID();
            Success = package.ReadBool();
            Amount = package.ReadInt();
        }

        public RequestDropResponse() {
            responseItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            InventoryHelper.WriteItemToPackage(responseItem, package);
            package.Write(sender);
            package.Write(Success);
            package.Write(Amount);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestDropResponse:");
            Log.LogDebug($"  switchItem: {responseItem != null}");
            InventoryHelper.PrintItem(responseItem);
        }
    }
}
