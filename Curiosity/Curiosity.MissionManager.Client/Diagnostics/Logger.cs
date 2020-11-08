using Curiosity.MissionManager.Client.Handler;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Curiosity.MissionManager.Client.Diagnostics
{
    public static class Logger
    {
        public static void Info(string msg)
        {
            WriteLine("INFO", msg);
        }

        public static void Success(string msg)
        {
            WriteLine("SUCCESS", msg);
        }

        public static void Warn(string msg)
        {
            WriteLine("WARN", msg);
        }

        public static void Error(string msg)
        {
            WriteLine("ERROR", msg);
        }

        public static void Error(Exception ex, string msg = "")
        {
            WriteLine("ERROR", $"{msg}\r\n{ex}");
        }

        public static void Verbose(string msg)
        {
            WriteLine("VERBOSE", msg);
        }

        public static void Debug(string msg)
        {
            if (Cache.Player == null) return;
            if (!Cache.Player.User.IsDeveloper) return;
            WriteLine("DEBUG", msg);
        }

        private static void WriteLine(string title, string msg)
        {
            try
            {
                MethodBase method = new StackTrace().GetFrame(1).GetMethod();
                string caller = $"{method.ReflectedType?.FullName}.{method.Name}";
                CitizenFX.Core.Debug.WriteLine($"{caller}: [{title}] {msg}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
