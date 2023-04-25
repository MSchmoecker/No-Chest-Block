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
                AppendPackage(package.TargetInventory, package);
                total += 1;
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

        public static bool GetChanges(Inventory inventory, out Dictionary<Vector2i, SlotPreview> preview) {
            if (inventory == null) {
                preview = null;
                return false;
            }

            preview = new Dictionary<Vector2i, SlotPreview>();

            if (PackageChanges.TryGetValue(inventory, out List<IRequest> packages)) {
                foreach (IRequest package in packages) {
                    if (package is RequestChestAdd requestChestAdd) {
                        if (requestChestAdd.dragItem == null) {
                            continue;
                        }

                        AddPreview(preview, requestChestAdd.toPos, requestChestAdd.dragItem, requestChestAdd.dragItem.m_stack);
                    } else if (package is RequestChestRemove requestChestRemove) {
                        Vector2i fromPos = requestChestRemove.fromPos;
                        Vector2i toPos = requestChestRemove.toPos;

                        if (inventory == package.SourceInventory) {
                            AddPreview(preview, fromPos, requestChestRemove.item, -requestChestRemove.dragAmount);

                            if (requestChestRemove.switchItem != null) {
                                AddPreview(preview, toPos, requestChestRemove.switchItem, requestChestRemove.switchItem.m_stack);
                            }
                        }

                        if (inventory == package.TargetInventory) {
                            AddPreview(preview, toPos, requestChestRemove.item, requestChestRemove.dragAmount);
                        }
                    }
                }

                return true;
            }

            preview = null;
            return false;
        }

        private static void AddPreview(Dictionary<Vector2i, SlotPreview> preview, Vector2i pos, ItemDrop.ItemData item, int amount) {
            if (!preview.TryGetValue(pos, out SlotPreview diff)) {
                diff = new SlotPreview(item, amount);
                preview.Add(pos, diff);
            } else {
                diff.amountDiff += amount;
            }
        }
    }
}
