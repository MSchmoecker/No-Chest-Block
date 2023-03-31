using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiUserChest {
    public static class InventoryHandler {
        public static void RPC_RequestItemAddResponse(long sender, ZPackage package) {
            HandleRPC(new RequestChestAddResponse(package), p => GetSourceInventory(p.SourceID), RPC_RequestItemAddResponse);
        }

        public static void RPC_RequestItemRemoveResponse(long sender, ZPackage package) {
            HandleRPC(new RequestChestRemoveResponse(package), p => GetInventory(p.sender, p.inventoryHash), RPC_RequestItemRemoveResponse);
        }

        public static void RPC_RequestDropResponse(long sender, ZPackage package) {
            HandleRPC(new RequestDropResponse(package), RPC_RequestDropResponse);
        }

        public static void RPC_RequestItemConsumeResponse(long sender, ZPackage package) {
            HandleRPC(new RequestConsumeResponse(package), RPC_RequestItemConsumeResponse);
        }

        public static void RPC_RequestItemMoveResponse(long sender, bool success) {
#if DEBUG
            Timer.Stop(nameof(RPC_RequestItemMoveResponse));
            Log.LogDebug($"RequestItemMoveResponse:");
            Log.LogDebug($"\tsuccess: {success}");
#endif
        }

        private static void HandleRPC<T>(T package, Func<T, Inventory> inventory, Container container,
            Action<Inventory, Container, T> handle) where T : IPackage {
            CallRPC(handle.Method.Name, package, () => handle.Invoke(inventory.Invoke(package), container, package));
        }

        private static void HandleRPC<T>(T package, Func<T, Inventory> inventory, Action<Inventory, T> handle) where T : IPackage {
            CallRPC(handle.Method.Name, package, () => handle.Invoke(inventory.Invoke(package), package));
        }

        private static void HandleRPC<T>(T package, Action<T> handle) where T : IPackage {
            CallRPC(handle.Method.Name, package, () => handle.Invoke(package));
        }

        private static void CallRPC(string timerName, IPackage package, Action action) {
#if DEBUG
            Timer.Stop(timerName);
            package.PrintDebug();
#endif

            action?.Invoke();
        }

        public static void RPC_RequestItemRemoveResponse(Inventory inventory, RequestChestRemoveResponse response) {
            Vector2i inventoryPos = response.inventoryPos;
            InventoryBlock.Get(inventory).ReleaseSlot(inventoryPos);

            if (response.Success && inventoryPos.x >= 0 && inventoryPos.y >= 0) {
                bool added = inventory.AddItemToInventory(response.responseItem, response.Amount, inventoryPos);

                if (!added) {
                    int dropAmount = response.Amount;
                    ItemDrop.ItemData exisingItem = inventory.GetItemAt(inventoryPos.x, inventoryPos.y);

                    if (exisingItem != null && InventoryHelper.IsSameItem(response.responseItem, exisingItem)) {
                        int maxStackSize = exisingItem.m_shared.m_maxStackSize;
                        int existingAmount = exisingItem.m_stack;
                        int freeSpace = maxStackSize - existingAmount;

                        exisingItem.m_stack += freeSpace;
                        dropAmount -= freeSpace;
                    }

                    DropItem(response.responseItem, dropAmount);
                }
            } else if (response.responseItem != null) {
                Inventory tmp = new Inventory("tmp", null, 1, 1);
                tmp.AddItem(response.responseItem);
                inventory.MoveItemToThis(tmp, response.responseItem);

                ItemDrop.ItemData notMovedItem = tmp.GetItemAt(0, 0);

                if (notMovedItem != null) {
                    DropItem(notMovedItem, notMovedItem.m_stack);
                }
            }

            UpdateGUIAfterPlayerMove(response.sender);
        }

        private static void RPC_RequestDropResponse(RequestDropResponse response) {
            DropItem(response.responseItem, response.responseItem?.m_stack ?? 0);
            UpdateGUIAfterPlayerMove(response.sender);
        }

        private static void UpdateGUIAfterPlayerMove(ZDOID sender) {
            if (InventoryGui.instance && Player.m_localPlayer && sender == Player.m_localPlayer.GetZDOID()) {
                InventoryGui.instance.UpdateCraftingPanel();
            }
        }

        private static void UpdateGUIAfterPlayerMove(Inventory inventory) {
            if (!InventoryGui.instance || !Player.m_localPlayer) {
                return;
            }

            if (Player.m_localPlayer.GetInventory().IsExtendedInventory(out List<Inventory> inventories)) {
                if (inventories.Contains(inventory)) {
                    InventoryGui.instance.UpdateCraftingPanel();
                }
            } else {
                if (inventory == Player.m_localPlayer.GetInventory()) {
                    InventoryGui.instance.UpdateCraftingPanel();
                }
            }
        }

        public static void DropItem(ItemDrop.ItemData item, int amount) {
            if (amount > 0 && item != null) {
                DropItem(Player.m_localPlayer, item, amount);
            }
        }

        private static void DropItem(Player player, ItemDrop.ItemData item, int amount) {
            Transform playerTransform = player.transform;

            ItemDrop drop = ItemDrop.DropItem(item, amount, playerTransform.position + playerTransform.forward + playerTransform.up, playerTransform.rotation);
            drop.OnPlayerDrop();
            drop.GetComponent<Rigidbody>().velocity = (playerTransform.forward + Vector3.up) * 5f;
            player.m_zanim.SetTrigger("interact");
            player.m_dropEffects.Create(playerTransform.position, Quaternion.identity);

            ItemDrop.ItemData dropData = drop.m_itemData;
            player.Message(MessageHud.MessageType.TopLeft, "$msg_dropped " + dropData.m_shared.m_name, dropData.m_stack, dropData.GetIcon());
        }

        public static void RPC_RequestItemAddResponse(Inventory inventory, RequestChestAddResponse response) {
            Vector2i inventoryPos = response.inventoryPos;
            ItemDrop.ItemData switchItem = response.switchItem;

            InventoryBlock.Get(inventory).ReleaseSlot(inventoryPos);
            InventoryPreview.RemovePackage(response);

            if (switchItem != null) {
                bool added = inventory.AddItemToInventory(switchItem, switchItem.m_stack, inventoryPos);

                if (!added) {
                    DropItem(switchItem, switchItem.m_stack);
                }
            }

            UpdateGUIAfterPlayerMove(GetSourceInventory(response.SourceID));
        }

        public static void RPC_RequestItemConsumeResponse(RequestConsumeResponse response) {
            Player player = Player.m_localPlayer;
            InventoryBlock.Get(player.GetInventory()).BlockConsume = false;

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

        internal static Inventory GetInventory(ZDOID targetId, int hash) {
            GameObject target = ZNetScene.instance.FindInstance(targetId);

            if (target.TryGetComponent(out Player player)) {
                if (player.GetInventory().IsExtendedInventory(out List<Inventory> inventories)) {
                    return inventories.FirstOrDefault(i => i.m_name.GetStableHashCode() == hash) ?? player.GetInventory();
                }

                return player.GetInventory();
            }

            if (target.TryGetComponent(out Container container)) {
                return container.GetInventory();
            }

            return null;
        }

        internal static Inventory GetSourceInventory(int id) {
            if (!PackageHandler.GetPackage(id, out IPackage package)) {
                return null;
            }

            if (package is RequestChestAdd packageAdd) {
                return packageAdd.sourceInventory;
            }

            return null;
        }

        internal static Inventory GetTargetInventory(int id) {
            if (!PackageHandler.GetPackage(id, out IPackage package)) {
                return null;
            }

            if (package is RequestChestAdd packageAdd) {
                return packageAdd.targetInventory;
            }

            return null;
        }
    }
}
