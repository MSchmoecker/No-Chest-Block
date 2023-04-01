using System;
using System.Collections.Generic;

namespace MultiUserChest {
    public static class PackageHandler {
        private static readonly Dictionary<int, IPackage> Packages = new Dictionary<int, IPackage>();
        private static readonly Random Random = new Random();

        public static int AddPackage(IPackage package) {
            int id = GetRandomId();
            Packages.Add(id, package);
            return id;
        }

        public static void RemovePackage(int id) {
            Packages.Remove(id);
        }

        private static int GetRandomId() {
            return Random.Next(int.MinValue, int.MaxValue);
        }

        public static bool GetPackage(int id, out IPackage package) {
            return Packages.TryGetValue(id, out package);
        }
    }
}
