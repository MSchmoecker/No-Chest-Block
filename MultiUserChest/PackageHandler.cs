using System;
using System.Collections.Generic;

namespace MultiUserChest {
    public static class PackageHandler {
        private static Dictionary<int, IPackage> packages = new Dictionary<int, IPackage>();
        private static readonly Random random = new Random();

        public static int AddPackage(IPackage package) {
            int id = GetRandomId();
            packages.Add(id, package);
            return id;
        }

        private static int GetRandomId() {
            return random.Next(int.MinValue, int.MaxValue);
        }

        public static bool GetPackage(int id, out IPackage package) {
            return packages.TryGetValue(id, out package);
        }
    }
}
