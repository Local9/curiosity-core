﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Commands;
using Curiosity.Systems.Server.Commands.Impl;
using Curiosity.Systems.Server.Database;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Extenstions;
using Curiosity.Systems.Server.Managers;
using Curiosity.Systems.Server.Web;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server
{
    public class CuriosityPlugin : BaseScript
    {
        const string CURIOSITY_VERSION = "v2.0.0.0001";
        private const string CONVAR_MISSING = "MISSING";

        public static CuriosityPlugin Instance { get; private set; }
        public static PlayerList PlayersList { get; private set; }
        public static int MaximumPlayers { get; } = 32;
        public static int ServerId { get; private set; }
        public static int SpawnLocationId { get; private set; }
        public static int SaveInterval { get; } = 1000 * 60 * 3;
        public static bool IsDebugging { get; private set; }
        public static bool IsMaintenanceActive { get; private set; }
        public static bool ServerReady { get; private set; }
        public static ulong DiscordGuildId { get; private set; }
        public static string DiscordBotKey { get; private set; }
        public static string DiscordUrl { get; private set; }
        public static string WebsiteUrl { get; private set; }
        public static Dictionary<int, CuriosityUser> ActiveUsers { get; } = new Dictionary<int, CuriosityUser>();
        public long LastSave { get; set; } = Date.Timestamp;

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportDictionary => Exports;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();

        public CuriosityPlugin()
        {
            Logger.Info("[CuriosityPlugin] Loading...");

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
                IsDebugging = API.GetConvar("diagnostics_debug", "false") == "true";

                if (IsDebugging)
                {
                    Logger.Warn($"----------------------------------------");
                    Logger.Warn($"------------ DEBUG ACTIVE --------------");
                    Logger.Warn($"----------------------------------------");
                }

                IsMaintenanceActive = API.GetConvar("server_live", "false") == "false";

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

                string serverKey = API.GetConvar("sv_licenseKey", CONVAR_MISSING);

                if (serverKey == CONVAR_MISSING)
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

                bool checkServerSetup = await Database.Store.ServerDatabase.CheckServerKey(ServerId, serverKey);

                if (!checkServerSetup)
                {
                    while (true)
                    {
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Logger.Error("!!!!!!!! Server not found in server table! !!!!!!!!!!");
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
                string curiosity = "Curiosity";

                if (tagArr.Length > 0)
                {
                    API.SetConvar("tags", $"{tags}, {curiosity}");
                }
                else
                {
                    API.SetConvar("tags", $"{curiosity}");
                }
                API.SetConvarServerInfo("Curiosity", CURIOSITY_VERSION);

                // Disable client side entity creation
                Function.Call((Hash)0x0071321B, "relaxed");
            }
            catch (Exception ex)
            {
                ErrorText errorText = new ErrorText();
                errorText.PrintErrorText();
            }
        }

        private void Load()
        {
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
            commands.Bind(typeof(DeveloperTools));

            Logger.Info($"[Managers] Successfully loaded in {loaded} manager(s)!");

            AttachTickHandler(SaveTask);

            EventRegistry["rconCommand"] += new Action<string, List<object>>(OnRconCommand);

            PlayersList = Players;

            ServerReady = true;
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
            if (commandName.ToLower() == "save")
            {

                Logger.Info("[Saves] Beginning `Save` operation on `Characters`.");

                try
                {
                    foreach (var users in ActiveUsers)
                    {
                        await SaveOperation(users.Value);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"{ex}");
                }

                Logger.Info("[Saves] Completed `Save` operation on `Characters`.");
            }

            if (commandName.ToLower() == "party")
            {
                if (args.Count == 0)
                {
                    Logger.Info($"Command Missing");
                    Logger.Info($"Commands: show, count");
                }
                else
                {
                    if ($"{args.ElementAt(0)}" == "count")
                    {
                        Logger.Info($"Number of Parties: {PartyManager.ActiveParties.Count}");
                    }

                    if ($"{args.ElementAt(0)}" == "show")
                    {
                        if (PartyManager.ActiveParties.Count == 0)
                        {
                            Logger.Info($"No active parties");
                        }
                        else
                        {
                            foreach (KeyValuePair<Guid, Party> p in PartyManager.ActiveParties)
                            {
                                Logger.Info($"GUID: {p.Key}");
                                Logger.Info($"::: Members :::");

                                foreach (PartyMember partyMember in p.Value.Members)
                                {
                                    if (partyMember.Leader)
                                    {
                                        Logger.Info($"Leader: {partyMember.Name}#{partyMember.Handle}");
                                    }
                                    else
                                    {
                                        Logger.Info($"Member: {partyMember.Name}#{partyMember.Handle}");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            try
            {
                API.CancelEvent();
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

        public async Task SaveOperation(CuriosityUser user)
        {
            await user.Character.Save();
        }

        private async Task SaveTask()
        {
            if (LastSave + SaveInterval < Date.Timestamp)
            {
                if (ActiveUsers.Count > 0)
                {
                    Logger.Info("[Saves] Beginning `Save` operation on `Characters`.");

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
                            ActiveUsers.Remove(playerHandle);

                            SessionState sessionState = SessionState.Loading;
                            QueueManager.session.TryRemove(player.Identifiers["license"], out sessionState);

                            activeUsersRemoved++;
                        }
                    }

                    Logger.Info($"[ActiveUsers] Removed {activeUsersRemoved} of {activeUsers}.");
                    LastSave = Date.Timestamp;
                }

                await Delay(1000);
            }
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
    }
}
