﻿using CitizenFX.Core;
using System;

namespace Curiosity.Systems.Server.Diagnostics
{
    class Logger : BaseScript
    {
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

        public static void Debug(string msg)
        {
            if (!CuriosityPlugin.IsDebugging) return;
            WriteLine("DEBUG", msg, ConsoleColor.DarkGray);
        }

        private static void WriteLine(string title, string msg, ConsoleColor color)
        {
            try
            {
                var m = $"[CURIOSITY][{title}] {msg}";
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
