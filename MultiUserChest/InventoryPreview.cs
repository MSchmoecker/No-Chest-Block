using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MultiUserChest {
    public static class InventoryPreview {
        private static readonly ConditionalWeakTable<Inventory, List<IPackage>> Packages = new ConditionalWeakTable<Inventory, List<IPackage>>();
        private static readonly List<IPackage> PackagesToRemove = new List<IPackage>();

        public static void AddPackage(IPackage package) {
            if (package is RequestChestAdd requestChestAdd) {
                if (Packages.TryGetValue(requestChestAdd.targetInventory, out List<IPackage> packages)) {
                    packages.Add(package);
                } else {
                    Packages.Add(requestChestAdd.targetInventory, new List<IPackage> { package });
                }
            }
        }

        public static void RemovePackage(IPackage package) {
            if (package is RequestChestAddResponse requestChestAddResponse) {
                Inventory targetInventory = InventoryHandler.GetTargetInventory(requestChestAddResponse.SourceID);

                if (targetInventory == null) {
                    Log.LogWarning($"Target inventory not found for {requestChestAddResponse.SourceID}");
                    return;
                }

                if (Packages.TryGetValue(targetInventory, out List<IPackage> packages)) {
                    PackagesToRemove.Add(packages.Find(p => p is RequestChestAdd requestChestAdd && requestChestAdd.ID == requestChestAddResponse.SourceID));
                    targetInventory.m_onChanged += () => RemoveQueuedPackages(targetInventory);
                }
            }
        }

        private static void RemoveQueuedPackages(Inventory inventory) {
            if (Packages.TryGetValue(inventory, out List<IPackage> packages)) {
                foreach (IPackage package in new List<IPackage>(PackagesToRemove)) {
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

            if (Packages.TryGetValue(inventory, out List<IPackage> packages)) {
                foreach (IPackage package in packages) {
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
