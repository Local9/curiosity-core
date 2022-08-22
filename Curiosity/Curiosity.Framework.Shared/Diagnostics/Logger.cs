namespace Curiosity.Framework.Shared.Diagnostics
{
    public static class Logger
    {
        static string _loggingLevel = GetResourceMetadata(GetCurrentResourceName(), "logging_level", 0);

        static bool ShowOutput(string level)
        {
            string lowercase = _loggingLevel.ToLower();
            if (lowercase == "all") return true;
            return (lowercase == level);
        }

        public static void Info(string msg)
        {
            if (ShowOutput("info"))
                Format($"^6[INFO] {msg}");
        }

        public static void Trace(string msg)
        {
            if (ShowOutput("trace"))
                Format($"^2[TRACE] {msg}");
        }

        public static void Warning(string msg)
        {
            if (ShowOutput("warn"))
                Format($"^3[WARN] {msg}");
        }

        public static void Debug(string msg)
        {
            if (ShowOutput("debug"))
                Format($"^4[DEBUG] {msg}");
        }

        public static void Debug(string msg, object obj)
        {
            if (!ShowOutput("debug")) return;
            Format($"^4[DEBUG] {msg}");
            Format($"^4[DEBUG] {JsonConvert.SerializeObject(obj, Formatting.None)}");
        }

        public static void Error(string msg)
        {
            if (ShowOutput("error"))
                Format($"^1[ERROR] {msg}");
        }

        public static void Error(Exception ex, string msg)
        {
            if (ShowOutput("error"))
                Format($"^1[ERROR] {msg}\r\n{ex}");
        }

        static void Format(string msg)
        {
            CitizenFX.Core.Debug.WriteLine($"{msg}");
        }

        public static void CriticalError(string msg)
        {
            Format($"^1[CRITICAL ERROR] {msg}");
        }

        public static void CriticalError(Exception ex, string msg)
        {
            Format($"^1[CRITICAL ERROR] {msg}\r\n{ex}");
        }
    }
}
