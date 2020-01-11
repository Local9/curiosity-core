using System;
using CitizenFX.Core.Native;

namespace Curiosity.Systems.Server.Diagnostics
{
    public class Logger
    {
        public static bool DebugEnabled { get; } = Convert.ToBoolean(API.GetConvar("diagnostics_debug", "false"));

        public static void Info(string msg)
        {
            WriteLine("INFO", msg, ConsoleColor.White);
        }

        public static void Success(string msg)
        {
            WriteLine("SUCCESS", msg, ConsoleColor.Green);
        }

        public static void Warn(string msg)
        {
            WriteLine("WARN", msg, ConsoleColor.Yellow);
        }

        public static void Error(string msg)
        {
            WriteLine("ERROR", msg, ConsoleColor.Red);
        }

        public static void Error(Exception ex, string msg = "")
        {
            WriteLine("ERROR", $"{msg}\r\n{ex}", ConsoleColor.Red);
        }

        public static void Verbose(string msg)
        {
            WriteLine("VERBOSE", msg, ConsoleColor.DarkGray);
        }

        public static void Debug(string message)
        {
            if (!DebugEnabled) return;

            WriteLine("Debug", message, ConsoleColor.Cyan);
        }

        private static void WriteLine(string title, string msg, ConsoleColor color)
        {
            try
            {
                var m = $"[{title}] {msg}";
                Console.ForegroundColor = color;
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {m}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}