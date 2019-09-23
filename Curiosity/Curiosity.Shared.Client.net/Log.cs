using System;
using CitizenFX.Core;
using Curiosity.Shared.Client.net.Enums;

namespace Curiosity.Shared.Client.net
{
    public static class Log
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

        private static void WriteLine(string title, string msg, ConsoleColor color)
        {
            try
            {
                var m = $"[{title}] {msg}";
#if SERVER
				Console.ForegroundColor = color;
				Console.WriteLine( $"{DateTime.Now:HH:mm:ss.fff} {m}" );
				Console.ResetColor();
#else
                Debug.WriteLine(m);
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void ToChat(string message)
        {
            BaseScript.TriggerEvent("curiosity:Client:Chat:Message", "", "#ffffff", message);
        }

        public static void Notification(string title, string subject, string message)
        {
            BaseScript.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_LESTER_DEATHWISH}", 1, title, subject, message, 2);
        }
    }
}
