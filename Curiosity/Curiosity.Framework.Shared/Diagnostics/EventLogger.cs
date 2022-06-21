using Lusive.Events.Diagnostics;

namespace Curiosity.Framework.Shared.Diagnostics
{
    public class EventLogger : IEventLogger
    {
        static string _loggingLevel = GetResourceMetadata(GetCurrentResourceName(), "fx_logging_level", 0);

        static bool ShowOutput(string level)
        {
            string lowercase = _loggingLevel.ToLower();
            if (lowercase == "all") return true;
            return (level == _loggingLevel);
        }

        public void Debug(params object[] values)
        {
            if (ShowOutput("debug"))
                CitizenFX.Core.Debug.WriteLine(Format(values));
        }

        public void Info(params object[] values)
        {
            if (ShowOutput("info"))
                CitizenFX.Core.Debug.WriteLine(Format(values));
        }

        public void Error(params object[] values)
        {
            if (ShowOutput("error"))
                CitizenFX.Core.Debug.WriteLine(Format(values));
        }

        public string Format(object[] values)
        {
            return $"[Events] {string.Join(", ", values)}";
        }
    }
}
