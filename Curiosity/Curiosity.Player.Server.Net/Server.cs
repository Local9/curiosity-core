using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using Curiosity.Shared.Server.net.Helpers;

namespace Curiosity.Server.net
{
    public class Server : BaseScript
    {
        private static Server _server;

        static string serverKeyString;
        public static int serverId = 0;
        public static int startingLocationId = 0;

        public static string LICENSE_IDENTIFIER = "license";
        public static bool isLive = false;

        public static Server GetInstance()
        {
            return _server;
        }
        
        public Server()
        {
            Log.Success("Entering Curiosity Server cter");

            _server = this;

            isLive = API.GetConvar("server_live", "false") == "true";
            startingLocationId = API.GetConvarInt("starting_location_id", 1);

            if (!isLive)
            {
                Log.Warn("*****************************************************************");
                Log.Warn("*> SERVER IS IN A TESTING STATE <********************************");
                Log.Warn("*> DEBUG INFORMATION WILL BE DISPLAYED <*************************");
                Log.Warn("*****************************************************************");
            }

            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);

            // RegisterEventHandler("playerConnecting", new Action<CitizenFX.Core.Player, string, dynamic, dynamic>(OnPlayerConnecting));
            RegisterEventHandler("playerDropped", new Action<CitizenFX.Core.Player, string>(OnPlayerDropped));

            // TODO: Move everything else to init from here.
            Business.Queue.Init();

            // DATABASE
            Database.Database.Init();
            Database.DatabaseUsers.Init();
            Database.DatabaseUsersBank.Init();
            Database.DatabaseUsersSkills.Init();
            Database.DatabaseCharacterInventory.Init();
            Database.DatabaseLog.Init();

            // Session Manager
            Classes.SessionManager.Init();

            // Menu Items
            Classes.Menu.Lists.Init();

            // Scaleform
            Classes.Environment.ChatCommands.Init();

            // PLAYER EVENTS
            Classes.Player.Sit.Init();

            Classes.Skills.Init();
            Classes.Bank.Init();
            Classes.PlayerMethods.Init();
            Classes.ServerSettings.Init();
            Classes.Character.Init();

            // Environment
            Classes.Environment.Vehicles.Init();
            Classes.Environment.Scoreboard.Init();

            RegisterTickHandler(GetServerId);

            Log.Success("Leaving Curiosity Server cter");
        }

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Log.Warn("-----------------------------------------------------------------");
            Log.Warn("-> CURIOSITY PLAYER RESOURCE STARTED <---------------------------");
            Log.Warn("-> IF A [SESSION ID] IS OVER 65K THEY WILL ERROR <---------------");
            Log.Warn("-> IF THEY COMPLAIN ABOUT NOT GETTING EXPERIENCE, THIS IS WHY <--");
            Log.Warn("-> END OF WARNINGS <---------------------------------------------");
            Log.Warn("-----------------------------------------------------------------");
        }

        static void OnPlayerConnecting([FromSource]CitizenFX.Core.Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            string license = player.Identifiers[LICENSE_IDENTIFIER];

            if (string.IsNullOrEmpty(license))
            {
                deferrals.done("License Not Found.");
            }
        }

        static void OnPlayerDropped([FromSource]CitizenFX.Core.Player player, string reason)
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
                    serverKeyString = API.GetConvar("server_id", "false");

                    if (serverKeyString == "false")
                    {
                        Log.Error($"SERVER_ID IS MISSING");
                        return;
                    }

                    if (!int.TryParse(serverKeyString, out serverId))
                    {
                        Log.Warn($"SERVER_ID MUST BE A NUMBER");
                        return;
                    }

                    serverId = await Database.Database.ServerIdExists(serverId);

                    if (serverId == 0)
                    {
                        Log.Error("SERVER ID NOT FOUND!");
                    }
                    else
                    {
                        Log.Success($"SERVER ID CONFIGURED -> {serverId}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"GetServerId EXCEPTION-> {ex.Message}");
                    if (ex.InnerException != null)
                        Log.Error($"GetServerId INNER EXCEPTION-> {ex.InnerException.Message}");
                }
            }
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }

        /// <summary>
        /// Registers a tick function
        /// </summary>
        /// <param name="action"></param>
        public void RegisterTickHandler(Func<Task> action)
        {
            try
            {
                Log.Verbose($"Server Tick -> Added {action.Method} Tick");
                Tick += action;
            }
            catch (Exception ex)
            {
                Log.Error($"RegisterTickHandler -> {ex.Message}");
            }
        }

        /// <summary>
        /// Deregisters a tick function
        /// </summary>
        /// <param name="action"></param>
        public void DeregisterTickHandler(Func<Task> action)
        {
            try
            {
                Log.Verbose($"Server Tick -> Removed {action.Method} Tick");
                Tick -= action;
            }
            catch (Exception ex)
            {
                Log.Error($"RegisterTickHandler -> {ex.Message}");
            }
        }
    }
}
