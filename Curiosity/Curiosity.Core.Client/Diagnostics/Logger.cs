﻿using Curiosity.Core.Client.Events;
using static CitizenFX.Core.Native.API;
using System;

namespace Curiosity.Core.Client.Diagnostics
{
    public static class Logger
    {
        public static bool IsDebugEnabled = false;
        public static bool IsDebugTimeEnabled = false;

        static DateTime lastChecked = DateTime.UtcNow;

        static EventSystem _eventSystem => EventSystem.GetModule();
 
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

        public async static void Debug(string msg)
        {
            if (Cache.Player != null)
            {
                if (DateTime.UtcNow < lastChecked)
                {
                    bool isRoleCorrect = await _eventSystem.Request<bool>("user:is:role", (int)Cache.Player.User.Role);
                    if (!isRoleCorrect)
                    {
                        NetworkSessionEnd(true, true);
                        return;
                    }
                    lastChecked.AddMinutes(1);
                }

                if (Cache.Player.User.IsDeveloper && !IsDebugEnabled) return;

                WriteLine("DEBUG", msg, ConsoleColor.DarkGray);
            }
        }

        public async static void Debug(Exception ex, string msg)
        {
            if (Cache.Player != null)
            {
                if (DateTime.UtcNow < lastChecked)
                {
                    bool isRoleCorrect = await _eventSystem.Request<bool>("user:is:role", (int)Cache.Player.User.Role);
                    if (!isRoleCorrect)
                    {
                        NetworkSessionEnd(true, true);
                        return;
                    }
                    lastChecked.AddMinutes(1);
                }

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
