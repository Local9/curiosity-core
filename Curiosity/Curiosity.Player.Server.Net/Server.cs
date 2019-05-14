using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace Curiosity.Server.net
{
    public class Server : BaseScript
    {
        private static Server _server;

        static string serverKeyString;
        public static int serverKey;
        public static int serverId = 0;

        public static Server GetInstance()
        {
            return _server;
        }
        
        public Server()
        {
            WriteConsoleLine("Entering Curiosity Server cter");

            // TODO: Move everything else to init from here.

            _server = this;

            Tick += GetServerId;

            WriteConsoleLine("Leaving Curiosity Server cter");
        }

        async Task GetServerId()
        {
            while (serverId == 0)
            {
                await Delay(1000);
                try
                {
                    serverKeyString = API.GetConvar("server_key", "false");

                    if (serverKeyString == "false")
                    {
                        WriteConsoleLine(string.Empty);
                        WriteConsoleLine($"SERVER_KEY IS MISSING");
                    }

                    if (!int.TryParse(serverKeyString, out serverKey))
                    {
                        WriteConsoleLine(string.Empty);
                        WriteConsoleLine($"SERVER_KEY MUST BE A NUMBER");
                    }

                    WriteConsoleLine($"SERVER KEY -> {serverKey}", true);

                    serverId = await Database.DatabaseSettings.GetInstance().GetServerId(serverKey);

                    if (serverId == 0)
                    {
                        WriteConsoleLine("SERVER ID NOT FOUND!");
                    }
                    else
                    {
                        WriteConsoleLine($"SERVER KEY CONFIGURED -> {serverId}", true);
                    }
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    WriteConsoleLine($"GetServerId EXCEPTION-> {ex.Message}");
                    if (ex.InnerException != null)
                        WriteConsoleLine($"GetServerId INNER EXCEPTION-> {ex.InnerException.Message}");
                }
            }
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }

        public static void WriteConsoleLine(string message, bool good = false)
        {
            Console.ForegroundColor = good ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(message.PadRight(Console.WindowWidth));
            Console.ResetColor();
        }

    }
}
