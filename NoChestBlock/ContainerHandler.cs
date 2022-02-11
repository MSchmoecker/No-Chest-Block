using System.Collections.Generic;

namespace NoChestBlock {
    public class ContainerHandler {
        public static void TakeAll(Container container) {
            Inventory playerInventory = Player.m_localPlayer.GetInventory();
            List<ItemDrop.ItemData> wanted = InventoryHelper.GetAllMoveableItems(container.GetInventory(), playerInventory);

            InventoryHandler.blockAllSlots = true;

            RequestTakeAll request = new RequestTakeAll(wanted);
            Timer.Start(request);

            if (container.m_nview != null) {
                container.m_nview.InvokeRPC("RequestTakeAllItems", request.WriteToPackage());
            }
        }

        public static RequestAdd AddItemToChest(Vector2i from, Vector2i to, int dragAmount, bool allowSwitch, Inventory playerInventory, Container container) {
            ItemDrop.ItemData item = playerInventory.GetItemAt(from.x, from.y).Clone();
            RequestAdd request = new RequestAdd(from, to, dragAmount, item, allowSwitch);

            playerInventory.RemoveItem(playerInventory.GetItemAt(from.x, from.y), dragAmount);
            InventoryHandler.BlockSlot(request.fromInventory);

            if (container != null && container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemAdd", request.WriteToPackage());
            }

            return request;
        }

        public static void RemoveItemFromChest(RequestRemove request, Container container) {
            InventoryHandler.BlockSlot(request.toInventory);

            if (container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemRemove", request.WriteToPackage());
            }
        }
    }
}
