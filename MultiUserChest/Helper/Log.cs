using System;
using System.IO;
using BepInEx.Logging;

namespace MultiUserChest {
    /// <summary>
    /// Helper class for properly logging from static contexts.
    /// </summary>
    public static class Log {
        private static ManualLogSource logSource;

        internal static void Init(ManualLogSource manualLogSource) {
            logSource = manualLogSource;

            string logPath = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location), Plugin.ModName + "-log.txt");

            if (File.Exists(logPath)) {
                File.Delete(logPath);
            }
        }

        public static void LogCodeInstruction(object data) {
            logSource.LogDebug(data);
        }

        public static void LogDebug(string data) {
#if FULL_DEBUG
            logSource.LogDebug(data);
#endif
        }

        public static void LogError(string data) {
            logSource.LogError(data);
        }

        public static void LogFatal(string data) {
            logSource.LogFatal(data);
        }

        public static void LogInfo(string data) {
            logSource.LogInfo(data);
        }

        public static void LogWarning(string data) {
            logSource.LogWarning(data);
        }
    }
}
