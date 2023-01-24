using System.Diagnostics;

namespace MultiUserChest {
#if DEBUG
    public class Timer {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        public static void Start(IPackage request) {
            request.PrintDebug();
            stopwatch.Restart();
        }

        public static void Stop(string method) {
            Log.LogDebug($"{method}: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
        }
    }
#endif
}
