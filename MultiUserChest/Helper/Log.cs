using BepInEx.Logging;
using UnityEngine;

namespace MultiUserChest {
    /// <summary>
    /// Helper class for properly logging from static contexts.
    /// </summary>
    public static class Log {
        private static ManualLogSource logSource;

        internal static void Init(ManualLogSource manualLogSource) {
            logSource = manualLogSource;
        }

        public static void LogCodeInstruction(object data) {
#if DEBUG
            if (logSource != null) {
                logSource.LogDebug(data);
            } else {
                Debug.Log(data);
            }
#endif
        }

        public static void LogDebug(string data) {
#if DEBUG
            if (logSource != null) {
                logSource.LogDebug(data);
            } else {
                Debug.Log(data);
            }
#endif
        }

        public static void LogError(string data) {
            if (logSource != null) {
                logSource.LogError(data);
            } else {
                Debug.LogError(data);
            }
        }

        public static void LogFatal(string data) {
            if (logSource != null) {
                logSource.LogFatal(data);
            } else {
                Debug.LogError(data);
            }
        }

        public static void LogInfo(string data) {
            if (logSource != null) {
                logSource.LogInfo(data);
            } else {
                Debug.Log(data);
            }
        }

        public static void LogWarning(string data) {
            if (logSource != null) {
                logSource.LogWarning(data);
            } else {
                Debug.LogWarning(data);
            }
        }
    }
}
