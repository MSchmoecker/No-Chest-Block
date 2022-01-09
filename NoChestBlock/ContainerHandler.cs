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

        public static void AddItemToChest(RequestAdd request, Inventory playerInventory, Container container) {
            playerInventory.RemoveItem(playerInventory.GetItemAt(request.fromInventory.x, request.fromInventory.y), request.dragAmount);
            InventoryHandler.BlockSlot(request.fromInventory);

            if (container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemAdd", request.WriteToPackage());
            }
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
