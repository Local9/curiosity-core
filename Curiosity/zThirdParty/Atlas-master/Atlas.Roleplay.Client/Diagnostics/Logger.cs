using System;

namespace Atlas.Roleplay.Client.Diagnostics
{
    public class Logger
    {
        public static bool DebugEnabled { get; set; } = true;

        public static void Info(object message)
        {
            CitizenFX.Core.Debug.WriteLine($"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] {message ?? "null"}");
        }

        public static void Debug(object message)
        {
            if (!DebugEnabled) return;

            Info($"~ {message ?? "null"}");
        }
    }
}