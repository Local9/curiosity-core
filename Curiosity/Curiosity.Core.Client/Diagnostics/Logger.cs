﻿using Curiosity.Core.Client.Events;
using Curiosity.Systems.Library.Enums;

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
            WriteLine("^6[INFO]", msg, ConsoleColor.White);
        }

        public static void Success(string msg)
        {
            WriteLine("^2[SUCCESS]", msg, ConsoleColor.Green);
        }

        public static void Warn(string msg)
        {
            WriteLine("^3[WARN]", msg, ConsoleColor.Yellow);
        }

        public static void Error(string msg)
        {
            WriteLine("^1[ERROR]", msg, ConsoleColor.Red);
        }

        public static void Error(Exception ex, string msg = "")
        {
            WriteLine("^1[ERROR]", $"{msg}\r\n{ex}", ConsoleColor.Red);
        }

        public async static void Debug(string msg)
        {
            // if (!IsDebugEnabled) return;

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
                    lastChecked.AddMinutes(10);
                }

                if (Cache.Player.User.Role != Role.DEVELOPER) return;

                WriteLine("^4[DEBUG]", msg, ConsoleColor.DarkGray);
            }
        }

        public async static void Debug(Exception ex, string msg)
        {
            // if (!IsDebugEnabled) return;

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
                    lastChecked.AddMinutes(10);
                }

                if (Cache.Player.User.Role == Role.DEVELOPER) return;

                WriteLine("^4[DEBUG]", $"{msg}\r\n{ex}", ConsoleColor.DarkGray);
            }
        }

        private static void WriteLine(string title, string msg, ConsoleColor color)
        {
            try
            {
                var m = $"{title} {msg}";
                CitizenFX.Core.Debug.WriteLine(m);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
