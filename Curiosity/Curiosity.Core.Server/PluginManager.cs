using Curiosity.Core.Server.Commands;
using Curiosity.Core.Server.Commands.Impl;
using Curiosity.Core.Server.Database;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Managers;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Curiosity.Core.Server
{
    public class PluginManager : BaseScript
    {
        const string CURIOSITY_VERSION = "v2.0.1.4974";
        private const string CONVAR_MISSING = "MISSING";
        private string SERVER_KEY;

        public static PluginManager Instance { get; private set; }
        public static StateBag GlobalStateBag { get; private set; }
        public static PlayerList PlayersList { get; private set; }
        public static int MaximumPlayers { get; } = 32;
        public static int ServerId { get; private set; }
        public static int SpawnLocationId { get; private set; }

        static string _hostName;
        public static string Hostname
        {
            get
            {
                string cleanName = _hostName
                                    .Replace("^0", "")
                                    .Replace("^1", "")
                                    .Replace("^2", "")
                                    .Replace("^3", "")
                                    .Replace("^4", "")
                                    .Replace("^5", "")
                                    .Replace("^6", "")
                                    .Replace("^7", "")
                                    .Replace("^8", "")
                                    .Replace("^9", "");
                return cleanName;
            }
            private set { _hostName = value; }
        }
        public static bool IsDebugging { get; private set; }
        public static bool IsMaintenanceActive { get; private set; }
        public static bool ServerReady { get; private set; }
        public static ulong DiscordGuildId { get; private set; }
        public static string DiscordBotKey { get; private set; }
        public static string DiscordUrl { get; private set; }
        public static string WebsiteUrl { get; private set; }
        public static ConcurrentDictionary<int, CuriosityUser> ActiveUsers { get; } = new ConcurrentDictionary<int, CuriosityUser>();
        public long LastSave { get; set; } = GetGameTimer();

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportDictionary => Exports;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();
        public static bool IsSupporterAccess
        {
            get
            {
                return API.GetConvarInt("supporter_access_only", 0) == 1;
            }
        }

        public static bool IsLive
        {
            get
            {
                return API.GetConvar("server_live", "false") == "true";
            }
        }

        public PluginManager()
        {
            Logger.Info("[CuriosityPlugin-CORE] Loading...");

            Instance = this;
            SetupConvars();

            ServerReady = false;

            async Task DatabaseTest()
            {
                try
                {
                    Logger.Info("Running DB Connection Test...");

                    await BaseScript.Delay(100);

                    DetachTickHandler(DatabaseTest);

                    using (var result = MySqlDatabase.mySQL.QueryResult("SELECT 'Success' as Success"))
                    {
                        ResultSet keyValuePairs = await result;

                        if (keyValuePairs.Count == 0)
                            Logger.Error("DB Connection Test Failure");

                        if (keyValuePairs[0]["Success"] == "Success")
                            Logger.Success("DB Connection Test Successful");
                    }

                    async Task LoadTask()
                    {
                        DetachTickHandler(LoadTask);
                        Load();
                    }

                    AttachTickHandler(LoadTask);
                }
                catch (Exception ex)
                {
                    Logger.Error("!!! Critical Error trying to test Database Connection !!!");
                    Logger.Error($"Message: {ex.Message}");
                }
            }

            AttachTickHandler(DatabaseTest);
        }

        private async void SetupConvars()
        {
            try
            {
                GlobalState["mode"] = "open";

                GlobalStateBag = GlobalState;

                IsDebugging = API.GetConvar("diagnostics_debug", "false") == "true";

                if (IsDebugging)
                {
                    Logger.Warn($"----------------------------------------");
                    Logger.Warn($"------------ DEBUG ACTIVE --------------");
                    Logger.Warn($"----------------------------------------");
                }

                IsMaintenanceActive = !IsLive;

                if (IsMaintenanceActive)
                {
                    Logger.Warn($"----------------------------------------");
                    Logger.Warn($"--------- MAINTENANCE ACTIVE -----------");
                    Logger.Warn($"----------------------------------------");
                }

                ulong defDiscordGuildId = 0;
                if (ulong.TryParse(API.GetConvar("discord_guild", "0"), out defDiscordGuildId))
                {
                    DiscordGuildId = defDiscordGuildId;
                    Logger.Success($"Discord Guild ID: {DiscordGuildId}");
                }
                else
                {
                    while (true)
                    {
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Logger.Error("! Convar 'discord_guild' is not set or is not a number! !");
                        Logger.Error("!!!!! Please set this value and restart the server! !!!!!");
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        await BaseScript.Delay(3000);
                    }
                }

                DiscordBotKey = API.GetConvar("discord_bot", CONVAR_MISSING);

                if (DiscordBotKey == CONVAR_MISSING)
                {
                    Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    Logger.Error("!!!!!!!!!!! Convar 'discord_bot' is not set! !!!!!!!!!!!!");
                    Logger.Error("!!!!! Please set this value and restart the server! !!!!!");
                    Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                }

                ServerId = API.GetConvarInt("server_id", 0);
                if (ServerId == 0)
                {
                    while (true)
                    {
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Logger.Error("! Convar 'server_id' is not set or is not a number! !");
                        Logger.Error("!!! Please set this value and restart the server! !!!");
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        await BaseScript.Delay(3000);
                    }
                }

                SERVER_KEY = API.GetConvar("sv_licenseKey", CONVAR_MISSING);
                Hostname = API.GetConvar("sv_hostname", $"sv_hostname missing");

                if (SERVER_KEY == CONVAR_MISSING)
                {
                    while (true)
                    {
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Logger.Error("!!!!!!!! Convar 'sv_licenseKey' is not set! !!!!!!!!!");
                        Logger.Error("!!! Please set this value and restart the server! !!!");
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        await BaseScript.Delay(3000);
                    }
                }

                DiscordUrl = API.GetConvar("discord_url", "discord_url not set");
                API.SetConvarServerInfo("Discord", DiscordUrl);

                WebsiteUrl = API.GetConvar("website_url", "https://lifev.net");
                API.SetConvarServerInfo("Website", WebsiteUrl);
                API.SetGameType(API.GetConvar("game_type", "Simulation"));
                API.SetMapName("Curiosity Framework");

                SpawnLocationId = API.GetConvarInt("starting_location_id", 1);

                string tags = API.GetConvar("tags", string.Empty);
                string[] tagArr = tags.Split(',');
                string curiosity = "Curiosity-Core";

                if (tagArr.Length > 0)
                {
                    API.SetConvar("tags", $"{tags}, {curiosity}");
                }
                else
                {
                    API.SetConvar("tags", $"{curiosity}");
                }
                API.SetConvarServerInfo("Curiosity-Core", CURIOSITY_VERSION);

                // Disable client side entity creation
                // Function.Call((Hash)0x0071321B, "relaxed"); // ONESYNC
                // CreateObject fails, CreateObjectNoOffset is fine

                // API.SetRoutingBucketEntityLockdownMode(0, "relaxed");

                for(int i = 0; i <= 5; i++)
                {
                    SetRoutingBucketPopulationEnabled(i, true);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void Load()
        {
            Logger.Info("[CURIOSITY-CORE]: Loading managers, please wait...");

            Assembly.GetExecutingAssembly().GetExportedTypes()
                .SelectMany(self => self.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                .Where(self => self.GetCustomAttribute(typeof(TickHandler), false) != null).ToList()
                .ForEach(self =>
                {
                    var type = self.DeclaringType;

                    if (type == null) return;

                    if (!TickHandlers.ContainsKey(type))
                    {
                        TickHandlers.Add(type, new List<MethodInfo>());
                    }

                    Logger.Debug($"[TickHandlers] {type.Name}::{self.Name}");

                    TickHandlers[type].Add(self);
                });

            var loaded = 0;

            foreach (var type in Assembly.GetExecutingAssembly().GetExportedTypes())
            {
                if (type.BaseType == null) continue;
                if (!type.BaseType.IsGenericType) continue;

                var generic = type.BaseType.GetGenericTypeDefinition();

                if (generic != typeof(Manager<>) || type == typeof(Manager<>)) continue;

                LoadManager(type);

                loaded++;
            }

            foreach (var manager in Managers)
            {
                var method = manager.Key.GetMethod("Begin", BindingFlags.Public | BindingFlags.Instance);

                method?.Invoke(manager.Value, null);
            }

            var commands = new CommandFramework();
            commands.Bind(typeof(ServerCommands));
            commands.Bind(typeof(StaffCommands));

            Logger.Info($"[Managers] Successfully loaded in {loaded} manager(s)!");

            AttachTickHandlers(this);

            EventRegistry["rconCommand"] += new Action<string, List<object>>(OnRconCommand);

            PlayersList = Players;

            ServerReady = true;

            Logger.Success($"[CURIOSITY-CORE] Load method has been completed.");
        }

        public static Player GetPlayer(int netID)
        {
            return PlayersList[netID];
        }

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }

        private async void OnRconCommand(string commandName, List<object> args)
        {

            try
            {
                API.CancelEvent();

                if (commandName.ToLower() == "save")
                {
                    Logger.Info($"<- Save Start ->");
                    SavePlayers(true);
                }

                if (commandName.ToLower() == "weather")
                {
                    WorldManager.GetModule().WeatherDebugOutput();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error when canceling event, possible changes made by FiveM Collective.");
            }
        }

        public object LoadManager(Type type)
        {
            if (GetManager(type) != null) return null;

            Logger.Debug($"Loading in manager of type `{type.FullName}`");

            Managers.Add(type, default(Type));

            var instance = Activator.CreateInstance(type);

            AttachTickHandlers(instance);
            Managers[type] = instance;

            return instance;
        }

        public bool IsLoadingManager<T>() where T : Manager<T>, new()
        {
            return Managers.FirstOrDefault(self => self.Key == typeof(T)).Value is bool == false;
        }

        public object GetManager(Type type)
        {
            return Managers.FirstOrDefault(self => self.Key == type).Value;
        }

        public T GetManager<T>() where T : Manager<T>, new()
        {
            return (T)Managers.FirstOrDefault(self => self.Key == typeof(T)).Value;
        }

        public async Task<string> RequestHttp(string url, string json, Dictionary<string, string> headers,
            string method = "GET")
        {
            var request = new Request();

            return (await request.Http(url, method, json, headers)).content;
        }

        public void AttachTickHandlers(object instance)
        {
            TickHandlers.TryGetValue(instance.GetType(), out var methods);

            methods?.ForEach(async self =>
            {
                var handler = (TickHandler)self.GetCustomAttribute(typeof(TickHandler));

                Tick += (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), instance, self);

                RegisteredTickHandlers.Add(instance.GetType());
            });
        }

        public async Task SaveOperation(CuriosityUser user)
        {
            int serverHandle = user.Handle;
            int playerId = API.GetPlayerPed($"{serverHandle}");

            if (API.DoesEntityExist(playerId))
            {
                Ped ped = new Ped(playerId);

                user.Character.LastPosition = new Position(ped.Position.X, ped.Position.Y, ped.Position.Z);
            }

            await user.Character.Save();
        }

        public static async Task ServerLoading()
        {
            while (!PluginManager.ServerReady)
            {
                await BaseScript.Delay(0);
            }
        }

        [TickHandler]
        private async Task SaveTask()
        {
            try
            {
                if ((GetGameTimer() - (1000 * 60) * 5) > LastSave)
                {
                    SavePlayers();

                    await Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Possible user left while running");
            }
        }

        private async Task SavePlayers(bool command = false)
        {
            if (ActiveUsers.Count > 0)
            {
                foreach (var users in ActiveUsers)
                {
                    try
                    {
                        await SaveOperation(users.Value);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                Dictionary<int, CuriosityUser> auCopy = new Dictionary<int, CuriosityUser>(ActiveUsers);

                foreach (KeyValuePair<int, CuriosityUser> kvp in auCopy)
                {
                    try
                    {
                        if (Players[kvp.Key] is null)
                        {
                            int playerHandle = kvp.Key;
                            int staffVehicle = kvp.Value.StaffVehicle;
                            int playerVehicle = kvp.Value.PersonalVehicle;
                            int playerBoat = kvp.Value.PersonalBoat;
                            int playerTrailer = kvp.Value.PersonalTrailer;
                            int playerPlane = kvp.Value.PersonalPlane;
                            int playerHelicopter = kvp.Value.PersonalHelicopter;

                            ActiveUsers.TryRemove(kvp.Key, out CuriosityUser cu);

                            if (staffVehicle > 0) EntityManager.EntityInstance.NetworkDeleteEntity(staffVehicle);
                            await Delay(100);
                            if (playerVehicle > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerVehicle);
                            await Delay(100);
                            if (playerBoat > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerBoat);
                            await Delay(100);
                            if (playerTrailer > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerTrailer);
                            await Delay(100);
                            if (playerPlane > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerPlane);
                            await Delay(100);
                            if (playerHelicopter > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerHelicopter);
                            await Delay(100);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                LastSave = GetGameTimer();
            }

            if (command)
            {
                Logger.Success($"Accounts saved");
            }
        }

        public List<Player> GetPlayersInRange(Vector3 position, float range)
        {
            Player[] players = PluginManager.PlayersList.ToArray();
            return players.Where(x => Vector3.Distance(x.Character.Position, position) <= range).ToList();
        }
    }
}
