using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NoChestBlock.Patches;
using UnityEngine;

namespace NoChestBlock {
    public class InventoryHandler {
        public static readonly List<Vector2i> blockedSlots = new List<Vector2i>();
        public static bool blockConsume;
        public static bool blockAllSlots;

        public static void RPC_RequestItemAddResponse(long sender, ZPackage package) {
            Timer.Stop("RPC_RequestItemAddResponse");
            RPC_RequestItemAddResponse(Player.m_localPlayer.GetInventory(), sender, package);
        }

        public static void RPC_RequestItemMoveResponse(long sender, bool success) {
            Timer.Stop("RPC_RequestItemMoveResponse");
            InventoryGui.instance.SetupDragItem(null, null, 0);
            Log.LogInfo($"RequestItemMoveResponse:");
            Log.LogInfo($"\tsuccess: {success}");
        }

        public static void RPC_RequestTakeAllItemsResponse(long sender, ZPackage package) {
            Timer.Stop("RPC_RequestTakeAllItemsResponse");
            RPC_RequestTakeAllItemsResponse(Player.m_localPlayer.GetInventory(), sender, package);
        }

        public static void RPC_RequestItemRemoveResponse(long sender, ZPackage package) {
            Timer.Stop("RPC_RequestItemRemoveResponse");
            RPC_RequestItemRemoveResponse(Player.m_localPlayer.GetInventory(), package);
        }

        public static void RPC_RequestItemRemoveResponse(Inventory inventory, ZPackage package) {
            RequestRemoveResponse response = new RequestRemoveResponse(package);
            response.PrintDebug();

            Vector2i inventoryPos = response.inventoryPos;

            ReleaseSlot(inventoryPos);

            if (response.success) {
                if (response.hasSwitched) {
                    ItemDrop.ItemData atSlot = inventory.GetItemAt(inventoryPos.x, inventoryPos.y);
                    inventory.RemoveItem(atSlot);
                }

                if (inventoryPos.x >= 0 && inventoryPos.y >= 0) {
                    inventory.AddItemToInventory(response.responseItem, response.amount, inventoryPos);
                } else {
                    DropItem(response.responseItem, response.amount);
                }
            }

            if (InventoryGui.instance != null) {
                UpdateGUIAfterMove();
            }
        }

        private static void UpdateGUIAfterMove() {
            InventoryGui.instance.SetupDragItem(null, null, 0);
            InventoryGui.instance.m_moveItemEffects.Create(InventoryGui.instance.transform.position, Quaternion.identity);
            InventoryGui.instance.UpdateCraftingPanel();
        }

        private static void DropItem(ItemDrop.ItemData responseItem, int amount) {
            Transform player = Player.m_localPlayer.transform;
            ItemDrop drop = ItemDrop.DropItem(responseItem, amount, player.position + player.forward + player.up, player.rotation);
            drop.OnPlayerDrop();
            drop.GetComponent<Rigidbody>().velocity = (player.forward + Vector3.up) * 5f;
            Player.m_localPlayer.m_zanim.SetTrigger("interact");
            Player.m_localPlayer.m_dropEffects.Create(player.position, Quaternion.identity);

            ItemDrop.ItemData dropData = drop.m_itemData;
            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$msg_dropped " + dropData.m_shared.m_name, dropData.m_stack, dropData.GetIcon());
        }

        public static void RPC_RequestItemAddResponse(Inventory inventory, long sender, ZPackage package) {
            RequestAddResponse response = new RequestAddResponse(package);
            response.PrintDebug();

            Vector2i inventoryPos = response.inventoryPos;
            ItemDrop.ItemData switchItem = response.switchItem;

            ReleaseSlot(inventoryPos);

            if (switchItem != null) {
                inventory.AddItemToInventory(switchItem, switchItem.m_stack, inventoryPos);
            }

            if (InventoryGui.instance != null) {
                InventoryGui.instance.SetupDragItem(null, null, 0);
            }
        }

        public static void RPC_RequestItemConsumeResponse(long sender, ZPackage package) {
            Timer.Stop("RPC_RequestItemConsumeResponse");

            RequestConsumeResponse response = new RequestConsumeResponse(package);
            response.PrintDebug();

            Player player = Player.m_localPlayer;
            blockConsume = false;

            if (response.item == null) {
                return;
            }

            if (!player.CanConsumeItem(response.item)) {
                return;
            }

            if (response.item.m_shared.m_consumeStatusEffect) {
                player.m_seman.AddStatusEffect(response.item.m_shared.m_consumeStatusEffect, true);
            }

            if (response.item.m_shared.m_food > 0.0) {
                player.EatFood(response.item);
            }
        }

        private static void RPC_RequestTakeAllItemsResponse(Inventory inventory, long sender, ZPackage package) {
            blockAllSlots = false;

            RequestTakeAll response = new RequestTakeAll(package);

            foreach (ItemDrop.ItemData item in response.items) {
                inventory.AddItemToInventory(item, item.m_stack, item.m_gridPos);
            }
        }

        public static void BlockSlot(Vector2i slot) {
            blockedSlots.Add(slot);
        }

        public static void ReleaseSlot(Vector2i slot) {
            blockedSlots.RemoveAll(i => i.x == slot.x && i.y == slot.y);
        }

        public static bool IsSlotBlocked(Vector2i slot) {
            return !blockAllSlots && blockedSlots.Contains(slot);
        }
    }
}