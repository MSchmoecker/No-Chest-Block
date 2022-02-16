using System;
using System.IO;
using BepInEx.Logging;
using Serilog;
using Serilog.Events;
using Logger = Serilog.Core.Logger;

namespace NoChestBlock {
    /// <summary>
    /// Helper class for properly logging from static contexts.
    /// </summary>
    public static class Log {
        private static ManualLogSource logSource;
        private static Logger fileLogger;

        internal static void Init(ManualLogSource manualLogSource) {
            logSource = manualLogSource;

            string logPath = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location), Plugin.ModName + "-log.txt");

            if (File.Exists(logPath)) {
                File.Delete(logPath);
            }

            fileLogger = new LoggerConfiguration()
                         .MinimumLevel.Verbose()
                         .WriteTo.File(logPath, LogEventLevel.Debug, flushToDiskInterval: TimeSpan.Zero)
                         .CreateLogger();
        }

        public static void LogDebug(string data) {
            fileLogger.Debug(data);
        }

        public static void LogError(string data) {
            logSource.LogError(data);
            fileLogger.Error(data);
        }

        public static void LogFatal(string data) {
            logSource.LogFatal(data);
            fileLogger.Fatal(data);
        }

        public static void LogInfo(string data) {
            logSource.LogInfo(data);
            fileLogger.Information(data);
        }

        public static void LogWarning(string data) {
            logSource.LogWarning(data);
            fileLogger.Warning(data);
        }
    }
}
