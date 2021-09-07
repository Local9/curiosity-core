﻿using System;

namespace Curiosity.Core.Client.Diagnostics
{
    public static class Logger
    {
        public static bool IsDebugEnabled = true;
        public static bool IsDebugTimeEnabled = false;

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
            if (Cache.Player != null)
            {
                if (Cache.Player.User.IsDeveloper && !IsDebugEnabled) return;

                WriteLine("DEBUG", msg, ConsoleColor.DarkGray);
            }
        }

        public static void Debug(Exception ex, string msg)
        {
            if (Cache.Player != null)
            {
                if (Cache.Player.User.IsDeveloper && !IsDebugEnabled) return;

                WriteLine("DEBUG", $"{msg}\r\n{ex}", ConsoleColor.DarkGray);
            }
        }

        private static void WriteLine(string title, string msg, ConsoleColor color)
        {
            try
            {
                var m = $"[{title}] {msg}";
                CitizenFX.Core.Debug.WriteLine(m);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
