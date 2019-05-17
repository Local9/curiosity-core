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

            _server = this;

            // TODO: Move everything else to init from here.

            // DATABASE
            Database.Database.Init();
            Database.DatabaseUsers.Init();
            Database.DatabaseUsersBank.Init();
            Database.DatabaseUsersSkills.Init();

            // PLAYER EVENTS
            Classes.Skills.Init();
            Classes.Bank.Init();
            Classes.PlayerMethods.Init();
            Classes.ServerSettings.Init();

            RegisterTickHandler(GetServerId);

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

                    serverId = await Database.Database.GetServerId(serverKey);

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
        /// <summary>
        /// Registers a tick function
        /// </summary>
        /// <param name="action"></param>
        public void RegisterTickHandler(Func<Task> action)
        {
            try
            {
                Debug.WriteLine($"Added {action} Tick");
                Tick += action;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RegisterTickHandler -> {ex.Message}");
            }
        }


    }
}
