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
        public static int serverId = 0;

        public static string LICENSE_IDENTIFIER = "license";
        public static bool isLive = false;

        public static Server GetInstance()
        {
            return _server;
        }
        
        public Server()
        {
            WriteConsoleLine("Entering Curiosity Server cter");

            _server = this;

            isLive = API.GetConvar("server_live", "false") == "true";

            if (!isLive)
            {
                WriteConsoleLine(string.Empty);
                WriteConsoleLine("*****************************************************************");
                WriteConsoleLine("*> SERVER IS IN A TESTING STATE <********************************");
                WriteConsoleLine("*> DEBUG INFORMATION WILL BE DISPLAYED <*************************");
                WriteConsoleLine("*****************************************************************");
                Console.WriteLine();
                Console.WriteLine();
            }

            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);

            RegisterEventHandler("playerConnecting", new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting));
            RegisterEventHandler("playerDropped", new Action<Player, string>(OnPlayerDropped));

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

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            WriteConsoleLine(string.Empty);
            WriteConsoleLine("-----------------------------------------------------------------");
            WriteConsoleLine("-> CURIOSITY PLAYER RESOURCE STARTED <---------------------------");
            WriteConsoleLine("-> IF A [SESSION ID] IS OVER 65K THEY WILL ERROR <---------------");
            WriteConsoleLine("-> IF THEY COMPLAIN ABOUT NOT GETTING EXPERIENCE, THIS IS WHY <--");
            WriteConsoleLine("-> END OF WARNINGS <---------------------------------------------");
            WriteConsoleLine("-----------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine();
        }

        void WriteConsoleLine(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(message.PadRight(Console.WindowWidth));
            Console.ResetColor();
        }

        static void OnPlayerConnecting([FromSource]Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            string license = player.Identifiers[LICENSE_IDENTIFIER];

            if (string.IsNullOrEmpty(license))
            {
                deferrals.done("License Not Found.");
            }
        }

        static void OnPlayerDropped([FromSource]Player player, string reason)
        {
            if (Classes.SessionManager.PlayerList.ContainsKey(player.Handle))
            {
                Classes.SessionManager.PlayerList[player.Handle].Dropped(reason);
            }
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

                    if (!int.TryParse(serverKeyString, out serverId))
                    {
                        WriteConsoleLine(string.Empty);
                        WriteConsoleLine($"SERVER_KEY MUST BE A NUMBER");
                    }

                    serverId = await Database.Database.ServerIdExists(serverId);

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
