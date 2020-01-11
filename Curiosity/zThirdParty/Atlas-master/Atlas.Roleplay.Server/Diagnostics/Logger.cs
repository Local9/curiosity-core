using System;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Server.Diagnostics
{
    public class Logger
    {
        public static bool DebugEnabled { get; } = Convert.ToBoolean(API.GetConvar("diagnostics_debug", "false"));

        public static void Info(object message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public static void Debug(object message)
        {
            if (!DebugEnabled) return;

            Info($"~ {message}");
        }
    }
}