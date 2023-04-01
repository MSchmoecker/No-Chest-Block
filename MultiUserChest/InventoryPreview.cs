using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MultiUserChest {
    public static class InventoryPreview {
        private static readonly ConditionalWeakTable<Inventory, List<IRequest>> PackageChanges = new ConditionalWeakTable<Inventory, List<IRequest>>();
        private static readonly List<IRequest> PackagesToRemove = new List<IRequest>();

        public static void AddPackage(IRequest package) {
            if (PackageChanges.TryGetValue(package.TargetInventory, out List<IRequest> packages)) {
                packages.Add(package);
            } else {
                PackageChanges.Add(package.TargetInventory, new List<IRequest> { package });
            }

            package.RequestID = PackageHandler.AddPackage(package);
        }

        public static void RemovePackage(IResponse package) {
            Inventory targetInventory = InventoryHandler.GetTargetInventory(package.SourceID);

            if (targetInventory == null) {
                Log.LogWarning($"Target inventory not found for {package.SourceID}");
                return;
            }

            if (PackageChanges.TryGetValue(targetInventory, out List<IRequest> packages)) {
                PackagesToRemove.Add(packages.Find(p => p.RequestID == package.SourceID));
                targetInventory.m_onChanged += () => RemoveQueuedPackages(targetInventory);
            }

            PackageHandler.RemovePackage(package.SourceID);
        }

        private static void RemoveQueuedPackages(Inventory inventory) {
            if (PackageChanges.TryGetValue(inventory, out List<IRequest> packages)) {
                foreach (IRequest package in new List<IRequest>(PackagesToRemove)) {
                    packages.Remove(package);
                    PackagesToRemove.Remove(package);
                }
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

                        if (!preview.TryGetValue(requestChestAdd.toPos, out SlotPreview diff)) {
                            diff = new SlotPreview(requestChestAdd.dragItem, requestChestAdd.dragItem.m_stack);
                            preview.Add(requestChestAdd.toPos, diff);
                        } else {
                            diff.amountDiff += requestChestAdd.dragItem.m_stack;
                        }
                    }
                }

                return true;
            }

            preview = null;
            return false;
        }
    }
}
