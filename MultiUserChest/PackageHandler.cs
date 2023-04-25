using System;
using System.Collections.Generic;

namespace MultiUserChest {
    public static class PackageHandler {
        private static readonly Dictionary<int, IPackage> Packages = new Dictionary<int, IPackage>();
        private static readonly Random Random = new Random();
        private static int total;

        public static int AddPackage(IPackage package) {
            int id = GetRandomId();
            Packages.Add(id, package);
            Log.LogDebug($"PackageHandler: Added package {id}, total packages: {++total}");
            return id;
        }

        public static void RemovePackage(int id) {
            Packages.Remove(id);
            Log.LogDebug($"PackageHandler: Removed package {id}, total packages: {--total}");
        }

        private static int GetRandomId() {
            return Random.Next(int.MinValue, int.MaxValue);
        }

        public static bool GetPackage<T>(int id, out T package) where T : IPackage {
            bool containsPackage = Packages.TryGetValue(id, out IPackage result);

            if (containsPackage) {
                package = (T)result;
                return true;
            }

            package = default;
            return false;
        }

        public static T GetPackage<T>(int id) where T : IPackage {
            if (Packages.TryGetValue(id, out IPackage result)) {
                return (T)result;
            }

            return default;
        }
    }
}
