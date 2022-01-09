using System.Diagnostics;

namespace NoChestBlock {
    public class Timer {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        public static void Start(IPackage request) {
            request.PrintDebug();
            stopwatch.Restart();
        }

        public static void Stop(string method) {
            Log.LogInfo($"{method}: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
        }
    }
}
