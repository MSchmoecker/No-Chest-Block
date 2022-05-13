using System.Diagnostics;

namespace MultiUserChest {
    public class Timer {
#if FULL_DEBUG
        private static readonly Stopwatch stopwatch = new Stopwatch();
#endif

        public static void Start(IPackage request) {
#if FULL_DEBUG
            request.PrintDebug();
            stopwatch.Restart();
#endif
        }

        public static void Stop(string method) {
#if FULL_DEBUG
            Log.LogDebug($"{method}: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
#endif
        }
    }
}
