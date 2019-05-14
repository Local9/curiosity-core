
using CitizenFX.Core;

namespace Curiosity.Shared.Client.net
{
    public static class Log
    {
        public static void LogMessage(string message)
        {
            Debug.WriteLine(message);
        }

        public static void Error(string message)
        {
            Debug.WriteLine($"ERROR -> {message}");
        }

        public static void Info(string message)
        {
            Debug.WriteLine($"INFO -> {message}");
        }

        public static void Debugg(string message)
        {
            Debug.WriteLine($"Debug -> {message}");
        }

        public static void ToChat(string message)
        {
            BaseScript.TriggerEvent("curiosity:Client:Chat:Message", "", "#ffffff", message);
        }

    }
}
