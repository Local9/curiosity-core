using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Threading.Tasks;

namespace Curiosity.Server.net
{
    public class Server : BaseScript
    {

        // Nitro : 712753920123863170

        private static Server _server;

        public static Random random = new Random(Guid.NewGuid().GetHashCode());

        static string serverKeyString;
        public static int serverId = 0;
        public static bool serverActive = false;
        public static bool showPlayerBlips = true;
        public static bool showPlayerLocation = true;
        public static int startingLocationId = 0;
        public static int minutesAFK = 15;
        public static string hostname
        {
            get
            {
                return API.GetConvar("sv_hostname", "unable to get hostname");
            }
        }

        public static bool IsBirthday = false;
        DateTime BirthdayDate = new DateTime(DateTime.Now.Year, 5, 15);

        public ExportDictionary ExportDictionary => Exports;

        const string CURIOSITY_VERSION = "v1.0.1.2550";
        public static string LICENSE_IDENTIFIER = "license";
        public static string DISCORD_IDENTIFIER = "discord";
        public static bool isLive
        {
            get
            {
                return API.GetConvar("server_live", "false") == "true";
            }
        }

        public static bool ShowExportMessages
        {
            get
            {
                return API.GetConvar("server_export_messages", "false") == "true";
            }
        }

        public static bool IsSupporterAccess
        {
            get
            {
                return API.GetConvarInt("supporter_access_only", 0) == 1;
            }
        }

        public static bool DEBUG
        {
            get
            {
                return API.GetConvarInt("server_debug_messages", 0) == 1;
            }
        }

        public static PlayerList players;

        static DateTime serverStarted;

        public static Server GetInstance()
        {
            return _server;
        }

        public Server()
        {
            Log.Success("Entering Curiosity Server cter");

            serverStarted = DateTime.Now;

            IsBirthday = (DateTime.Now.Date == BirthdayDate);

            players = Players;

            _server = this;

            GlobalState["mode"] = "open";

            startingLocationId = API.GetConvarInt("starting_location_id", 1);
            showPlayerBlips = API.GetConvarInt("player_blips", 1) == 1;
            showPlayerLocation = API.GetConvarInt("player_location_display", 1) == 1;
            minutesAFK = API.GetConvarInt("player_afk_timer", 15);

            if (IsSupporterAccess)
            {
                Log.Info("*****************************************************************");
                Log.Info("*> SERVER IN SUPPORTER ONLY STATE <******************************");
                Log.Info("*****************************************************************");
            }

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
            RegisterEventHandler("curiosity:server:special", new Action<CitizenFX.Core.Player>(OnSpecialDay));
            // RegisterEventHandler("rconCommand", new Action<string, List<object>>(OnRconCommand));

            // TODO: Move everything else to init from here.
            Business.Discord.Init();

            // DATABASE
            Database.Database.Init();
            Database.DatabaseUsers.Init();
            Database.DatabaseUsersBank.Init();
            Database.DatabaseUsersSkills.Init();
            Database.DatabaseCharacterInventory.Init();
            Database.DatabaseLog.Init();
            Database.DatabaseVehicles.Init();
            Database.DatabaseMission.Init();
            Database.Config.Init();

            // Classes.Missions.Init();
            // Business.Queue.Init();

            // Session Manager
            Classes.SessionManager.Init();

            // Menu Items
            Classes.Menu.Lists.Init();

            // Scaleform
            Classes.Environment.ChatCommands.Init();

            // PLAYER EVENTS
            Classes.Player.Sit.Init();
            Classes.Environment.TriggerEventForMultipule.Init();

            Classes.Skills.Init();
            Classes.Bank.Init();
            Classes.PlayerMethods.Init();
            Classes.ServerSettings.Init();
            Classes.Character.Init();

            // Environment
            Classes.Environment.Vehicles.Init();
            Classes.Environment.Scoreboard.Init();
            Classes.Environment.ChatService.Init();
            Classes.Environment.PolmavEvents.Init();
            // Classes.Environment.InstanceChecker.Init();
            // Classes.Environment.Trains.Init();
            // weather and time
            // Classes.Environment.WeatherSystems.Init(); // DEPRECATED
            // Classes.Environment.WorldTimeCycle.Init(); // DEPRECATED
            Classes.Environment.WorldSeasonCycle.Init();

            // menu options
            Classes.Menu.Player.Init();
            ServerExports.ExportFuncs.Init();

            // Config
            Classes.DiscordWrapper.Init();

            RegisterTickHandler(GetServerId);
            // ServerUpTime();
            //RegisterTickHandler(InstanceChecker);

            string tags = API.GetConvar("tags", string.Empty);
            string[] tagArr = tags.Split(',');
            string curiosity = "Curiosity";

            if (tagArr.Length > 0)
            {
                API.SetConvar("tags", $"{tags}, {curiosity}");
            }
            else
            {
                API.SetConvar("tags", $"{curiosity}");
            }

            API.SetConvarServerInfo("Map", $"Los Santos");
            API.SetConvarServerInfo("Curiosity", CURIOSITY_VERSION);

            Business.BusinessUser.BanManagement();

            if (Server.isLive)
                RegisterTickHandler(SentStartupMessage);

            Log.Success("Leaving Curiosity Server cter");
        }

        private void OnSpecialDay([FromSource]Player player)
        {
            IsBirthday = (DateTime.Now.Date == BirthdayDate);
            player.TriggerEvent("curiosity:client:special", IsBirthday);
        }

        private static async void ServerUpTime()
        {
            long started = API.GetGameTimer();
            while ((API.GetGameTimer() - started) > 60000)
            {
                DateTime dateTimeRun = DateTime.Now;
                TimeSpan timeSpan = dateTimeRun - serverStarted;
                long numberOfTicks = Math.Abs(timeSpan.Ticks / TimeSpan.TicksPerMillisecond);
                long hours = numberOfTicks / (1000 * 60 * 60);
                long minutes = (numberOfTicks - (hours * 60 * 60 * 1000)) / (1000 * 60);
                API.SetConvarServerInfo("Uptime", $"{hours:00}:{minutes:00}");
                await BaseScript.Delay(30000);
            }
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
                Classes.SessionManager.PlayerList[player.Handle].Dropped(player.Handle, reason);
            }
        }

        async Task SentStartupMessage()
        {
            await Delay(5000);
            if (Classes.DiscordWrapper.isConfigured)
            {
                await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, hostname, "Server Startup Initated", "Server has started, accepting players soon.", Enums.Discord.DiscordColor.Green);
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
                    Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "GetServerId", $"{ex}");

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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "RegisterTickHandler", $"{ex}");
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "DeregisterTickHandler", $"{ex}");
                Log.Error($"RegisterTickHandler -> {ex.Message}");
            }
        }
    }
}
