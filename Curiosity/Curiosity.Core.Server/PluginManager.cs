using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Commands;
using Curiosity.Core.Server.Commands.Impl;
using Curiosity.Core.Server.Database;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Managers;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Entity;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Curiosity.Core.Server
{
    public class PluginManager : BaseScript
    {
        const string CURIOSITY_VERSION = "v2.0.0.0001";
        private const string CONVAR_MISSING = "MISSING";
        private string SERVER_KEY;

        public static PluginManager Instance { get; private set; }
        public static PlayerList PlayersList { get; private set; }
        public static int MaximumPlayers { get; } = 32;
        public static int ServerId { get; private set; }
        public static int SpawnLocationId { get; private set; }
        public static string Hostname { get; private set; }
        public static bool IsDebugging { get; private set; }
        public static bool IsMaintenanceActive { get; private set; }
        public static bool ServerReady { get; private set; }
        public static ulong DiscordGuildId { get; private set; }
        public static string DiscordBotKey { get; private set; }
        public static string DiscordUrl { get; private set; }
        public static string WebsiteUrl { get; private set; }
        public static ConcurrentDictionary<int, CuriosityUser> ActiveUsers { get; } = new ConcurrentDictionary<int, CuriosityUser>();
        public DateTime LastSave { get; set; } = DateTime.Now;

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

                IsDebugging = API.GetConvar("diagnostics_debug", "false") == "true";

                if (IsDebugging)
                {
                    Logger.Warn($"----------------------------------------");
                    Logger.Warn($"------------ DEBUG ACTIVE --------------");
                    Logger.Warn($"----------------------------------------");
                }

                IsMaintenanceActive = IsLive;

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

                WebsiteUrl = API.GetConvar("website_url", "website_url not set");
                API.SetConvarServerInfo("Website", WebsiteUrl);
                API.SetGameType(API.GetConvar("game_type", "game_type not set"));
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
                Function.Call((Hash)0x0071321B, "relaxed"); // ONESYNC
                // CreateObject fails, CreateObjectNoOffset is fine
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

                if (commandName.ToLower() == "missions")
                {
                    Logger.Info($"<- Mission Manager Start ->");
                    Logger.Info($"Active: {MissionManager.ActiveMissions.Count()}");
                    Logger.Info($"Assistance Requests: {MissionManager.ActiveMissions.Where(x => x.Value.AssistanceRequested).Count()}");
                    Logger.Info($"<- Mission Manager End ->");
                }

                if (commandName.ToLower() == "mission")
                {
                    if (MissionManager.ActiveMissions.Count > 0)
                    {
                        ConcurrentDictionary<int, MissionData> missions = MissionManager.ActiveMissions;

                        if (missions.Count > 1)
                        {
                            if (args.Count == 0)
                            {
                                Logger.Info($"Must pass Players Server ID, too many missions to print");
                            }
                            else
                            {
                                int idx = (int)args[0];
                                Logger.Info($"\n{missions[idx]}");
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<int, MissionData> data in missions)
                            {
                                Logger.Info($"\nPlayer Handle: {data.Key}\n{data.Value}");
                            }
                        }
                    }
                    else
                    {
                        Logger.Info($"No active missions");
                    }
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
            if (DateTime.Now.Subtract(LastSave).TotalMinutes >= 5)
            {
                if (ActiveUsers.Count > 0)
                {
                    // Logger.Debug("[Saves] Beginning `Save` operation on `Characters`.");

                    foreach (var users in ActiveUsers)
                    {
                        await SaveOperation(users.Value);
                    }

                    // Added is a playerDropped event is not received
                    int activeUsers = ActiveUsers.Count;
                    int activeUsersRemoved = 0;

                    foreach (Player player in Players)
                    {
                        int playerHandle = int.Parse(player.Handle);
                        if (!ActiveUsers.ContainsKey(playerHandle))
                        {
                            CuriosityUser curiosityUser = ActiveUsers[playerHandle];

                            ActiveUsers.TryRemove(playerHandle, out CuriosityUser old);
                            QueueManager.session.TryRemove(player.Identifiers["license"], out SessionState sessionState);

                            bool userHadMission = MissionManager.ActiveMissions.ContainsKey(playerHandle);

                            if (userHadMission)
                            {
                                MissionData mission = MissionManager.ActiveMissions[playerHandle];
                                foreach (int partyMember in mission.PartyMembers)
                                {
                                    EventSystem.GetModule().Send("mission:backup:completed", partyMember);
                                }
                            }

                            EntityManager.EntityInstance.NetworkDeleteEntity(curiosityUser.PersonalVehicle);
                            MissionManager.FailureTracker.TryRemove(playerHandle, out int numFailed);
                            MissionManager.ActiveMissions.TryRemove(playerHandle, out MissionData oldMission);

                            activeUsersRemoved++;
                        }
                    }

                    // Logger.Debug($"[ActiveUsers] Removed {activeUsersRemoved} of {activeUsers}.");
                    LastSave = DateTime.Now;
                }

                await Delay(1000);
            }
        }
    }
}
