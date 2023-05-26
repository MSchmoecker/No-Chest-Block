using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MultiUserChest {
    public static class InventoryPreview {
        // private static readonly ConditionalWeakTable<Inventory, List<IRequest>> PackageChanges = new ConditionalWeakTable<Inventory, List<IRequest>>();
        public static readonly Dictionary<Inventory, List<IRequest>> PackageChanges = new Dictionary<Inventory, List<IRequest>>();
        public static readonly Dictionary<Inventory, List<IRequest>> ToRemovePackages = new Dictionary<Inventory, List<IRequest>>();

        public static void AddPackage(IRequest package) {
            package.RequestID = PackageHandler.AddPackage(package);
            AppendPackage(package.SourceInventory, package);
            AppendPackage(package.TargetInventory, package);
        }

        private static void AppendPackage(Inventory inventory, IRequest package) {
            if (PackageChanges.TryGetValue(inventory, out List<IRequest> packages)) {
                if (!packages.Contains(package)) {
                    packages.Add(package);
                }

                Log.LogDebug($"InventoryPreview: Appended package {package.RequestID} to inventory {inventory.m_name}, total packages: {packages.Count}");
            } else {
                PackageChanges.Add(inventory, new List<IRequest> { package });
                inventory.m_onChanged += () => RemoveQueuedPackages(inventory);

                Log.LogDebug($"InventoryPreview: Added package {package.RequestID} to inventory {inventory.m_name}, total packages: 1 (new)");
            }
        }

        public static void RemovePackage(IResponse package) {
            RemovePackage(InventoryHandler.GetSourceInventory(package.SourceID), package);
            RemovePackage(InventoryHandler.GetTargetInventory(package.SourceID), package);
            PackageHandler.RemovePackage(package.SourceID);
        }

        private static void RemovePackage(Inventory inventory, IResponse package) {
            if (inventory == null) {
                return;
            }

            InventoryOwner owner = InventoryOwner.GetOwner(inventory);
            IRequest packageToRemove = null;

            if (PackageChanges.TryGetValue(inventory, out List<IRequest> packages)) {
                packageToRemove = packages.Find(p => p.RequestID == package.SourceID);
            }

            if (packageToRemove == null) {
                return;
            }

            if (ToRemovePackages.TryGetValue(inventory, out List<IRequest> toRemovePackages)) {
                toRemovePackages.Add(packageToRemove);
            } else {
                ToRemovePackages.Add(inventory, new List<IRequest> { packageToRemove });
            }

            bool isSelfOwner = owner != null && owner.IsValid() && owner.ZNetView.IsOwner();

            // a response without success isn't followed by a changed event, thus we have to remove it immanently
            if (isSelfOwner || !package.Success) {
                RemoveQueuedPackages(inventory);
            }
        }

        private static void RemoveQueuedPackages(Inventory inventory) {
            if (ToRemovePackages.TryGetValue(inventory, out List<IRequest> packages) && packages.Count > 0) {
                foreach (IRequest package in new List<IRequest>(packages)) {
                    if (PackageChanges.TryGetValue(inventory, out List<IRequest> packageChanges)) {
                        packageChanges.Remove(package);
                    }

                    packages.Remove(package);
                    Log.LogDebug($"InventoryPreview: Removed package {package.RequestID} from inventory {inventory.m_name}, total packages: {packages.Count}");
                }
            }
        }

        public static bool GetChanges(Inventory inventory, out SlotPreview preview) {
            if (inventory == null) {
                preview = null;
                return false;
            }

            preview = new SlotPreview(inventory);

            if (PackageChanges.TryGetValue(inventory, out List<IRequest> packages)) {
                foreach (IRequest package in packages) {
                    if (package is RequestChestAdd requestChestAdd) {
                        if (requestChestAdd.dragItem == null) {
                            continue;
                        }

                        ItemDrop.ItemData switchItem = null;

                        if (requestChestAdd.allowSwitch) {
                            switchItem = package.TargetInventory.GetItemAt(requestChestAdd.toPos.x, requestChestAdd.toPos.y);
                        }

                        if (inventory == package.SourceInventory) {
                            preview.Add(requestChestAdd.dragItem.m_gridPos, switchItem);
                        }

                        if (inventory == package.TargetInventory) {
                            preview.Remove(requestChestAdd.toPos, switchItem);
                            preview.Add(requestChestAdd.toPos, requestChestAdd.dragItem);
                        }
                    } else if (package is RequestChestRemove requestChestRemove) {
                        Vector2i fromPos = requestChestRemove.fromPos;
                        Vector2i toPos = requestChestRemove.toPos;

                        if (inventory == package.SourceInventory) {
                            preview.Remove(fromPos, requestChestRemove.item, requestChestRemove.dragAmount);
                            preview.Add(fromPos, requestChestRemove.switchItem);
                        }

                        if (inventory == package.TargetInventory) {
                            preview.Add(toPos, requestChestRemove.item, requestChestRemove.dragAmount);
                        }
                    } else if (package is RequestMove requestMove) {
                        if (preview.GetSlot(requestMove.fromPos, out var exising) && requestMove.itemHash != exising.PrefabName().GetStableHashCode()) {
                            continue;
                        }

                        ItemDrop.ItemData switchItem = requestMove.TargetInventory.GetItemAt(requestMove.toPos.x, requestMove.toPos.y);

                        if (switchItem != null && !InventoryHelper.IsSameItem(requestMove.item, switchItem)) {
                            if (requestMove.dragAmount == requestMove.item.m_stack) {
                                preview.Remove(requestMove.fromPos, requestMove.item, requestMove.dragAmount);
                                preview.Remove(requestMove.toPos, switchItem, switchItem.m_stack);
                                preview.Add(requestMove.fromPos, switchItem, switchItem.m_stack);
                                preview.Add(requestMove.toPos, requestMove.item, requestMove.dragAmount);
                            }
                        } else {
                            int possibleAmount = Mathf.Max(0, requestMove.dragAmount);

                            if (switchItem != null) {
                                possibleAmount = Mathf.Min(switchItem.m_shared.m_maxStackSize - switchItem.m_stack, requestMove.dragAmount);
                            }

                            preview.Remove(requestMove.fromPos, requestMove.item, possibleAmount);
                            preview.Add(requestMove.toPos, requestMove.item, possibleAmount);
                        }
                    }
                }

                return preview.HasChanges();
            }

            preview = null;
            return false;
        }
    }
}
