using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MultiUserChest {
    public static class InventoryPreview {
        // private static readonly ConditionalWeakTable<Inventory, List<IRequest>> PackageChanges = new ConditionalWeakTable<Inventory, List<IRequest>>();
        public static readonly Dictionary<Inventory, List<IRequest>> PackageChanges = new Dictionary<Inventory, List<IRequest>>();
        private static int total;

        public static void AddPackage(IRequest package) {
            package.RequestID = PackageHandler.AddPackage(package);
            Log.LogDebug($"InventoryPreview: Added package {package.RequestID}, total packages: {total}");

            if (package is RequestChestAdd) {
                AppendPackage(package.SourceInventory, package);
                AppendPackage(package.TargetInventory, package);
                total += 2;
            } else if (package is RequestChestRemove) {
                AppendPackage(package.SourceInventory, package);
                AppendPackage(package.TargetInventory, package);
                total += 2;
            }
        }

        private static void AppendPackage(Inventory inventory, IRequest package) {
            if (PackageChanges.TryGetValue(inventory, out List<IRequest> packages)) {
                packages.Add(package);
                Log.LogDebug($"InventoryPreview: Appended package {package.RequestID} to inventory {inventory}, total packages: {packages.Count}");
            } else {
                PackageChanges.Add(inventory, new List<IRequest> { package });
            }
        }

        public static void RemovePackage(IResponse package) {
            RemovePackage(InventoryHandler.GetSourceInventory(package.SourceID), package);
            RemovePackage(InventoryHandler.GetTargetInventory(package.SourceID), package);

            PackageHandler.RemovePackage(package.SourceID);
            Log.LogDebug($"InventoryPreview: Removed package {package.SourceID}, total packages: {--total}");
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

            if (!owner.IsValid() || packageToRemove == null) {
                return;
            }

            if (owner.ZNetView.IsOwner()) {
                RemoveQueuedPackages(inventory, packageToRemove);
            } else {
                inventory.m_onChanged += () => RemoveQueuedPackages(inventory, packageToRemove);
            }
        }

        private static void RemoveQueuedPackages(Inventory inventory, IRequest package) {
            if (PackageChanges.TryGetValue(inventory, out List<IRequest> packages)) {
                packages.Remove(package);
                Log.LogDebug($"InventoryPreview: Removed package {package.RequestID} from inventory {inventory}, total packages: {packages.Count}");
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
                            preview.Add(requestChestAdd.toPos, requestChestAdd.dragItem);
                            preview.Remove(requestChestAdd.toPos, switchItem);
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
                    }
                }

                return preview.HasChanges();
            }

            preview = null;
            return false;
        }
    }
}
