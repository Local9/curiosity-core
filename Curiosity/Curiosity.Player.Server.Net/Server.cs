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
        public static bool serverActive = false;
        public static int startingLocationId = 0;

        public static string LICENSE_IDENTIFIER = "license";
        public static string DISCORD_IDENTIFIER = "discord";
        public static bool isLive = false;
        public static PlayerList players;

        public static Server GetInstance()
        {
            return _server;
        }
        
        public Server()
        {
            Log.Success("Entering Curiosity Server cter");

            players = Players;

            _server = this;

            isLive = API.GetConvar("server_live", "false") == "true";
            startingLocationId = API.GetConvarInt("starting_location_id", 1);

            API.SetConvar("sv_authMaxVariance", "");
            API.SetConvar("sv_authMinTrust", "");

            API.SetConvarServerInfo("Discord", API.GetConvar("discord_url", "discord_url not set"));

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
            Business.Discord.Init();

            // DATABASE
            Database.Database.Init();
            Database.DatabaseUsers.Init();
            Database.DatabaseUsersBank.Init();
            Database.DatabaseUsersSkills.Init();
            Database.DatabaseCharacterInventory.Init();
            Database.DatabaseLog.Init();
            Database.Config.Init();

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
            // Classes.Environment.InstanceChecker.Init();

            // Config
            Classes.DiscordWrapper.Init();

            RegisterTickHandler(GetServerId);
            //RegisterTickHandler(InstanceChecker);

            System.Timers.Timer aTimer = new System.Timers.Timer(1000 * 60 * 10); // MAYBE JUST MAYBE Future Ant can see that marking this with a flag so other servers don't run it would of been a good idea...
            int lastHour = DateTime.Now.Hour;
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(Business.BusinessUser.BanManagement);

            if (Server.isLive)
                RegisterTickHandler(SentStartupMessage);

            Log.Success("Leaving Curiosity Server cter");
        }

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Log.Success("-----------------------------------------------------------------");
            Log.Success("-> CURIOSITY SERVER RESOURCE STARTING UP <-----------------------");
            Log.Success("-----------------------------------------------------------------");
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

        async Task SentStartupMessage()
        {
            await Delay(5000);
            if (Classes.DiscordWrapper.isConfigured)
            {
                await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, API.GetConvar("server_message_name", "SERVERNAME_MISSING"), "Server Startup Initated", "Server has started, accepting players soon.", Enums.Discord.DiscordColor.Green);
                DeregisterTickHandler(SentStartupMessage);
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
                        serverActive = true;
                        DeregisterTickHandler(GetServerId);
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
