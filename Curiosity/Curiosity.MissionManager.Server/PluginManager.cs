using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Server.Commands;
using Curiosity.MissionManager.Server.Diagnostics;
using Curiosity.MissionManager.Server.Managers;
using Curiosity.MissionManager.Server.Web;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Server
{
    public class PluginManager : BaseScript
    {
        private const string CONVAR_MISSING = "MISSING";

        internal static PluginManager Instance { get; private set; }
        public static PlayerList PlayersList { get; private set; }
        public static bool ServerReady { get; private set; } = false;
        public static bool IsDebugging { get; private set; } = false;
        public static ulong DiscordGuildId { get; private set; }
        public static string DiscordBotKey { get; private set; }
        public static string DiscordUrl { get; private set; }
        public static ConcurrentDictionary<int, CuriosityUser> ActiveUsers { get; } = new ConcurrentDictionary<int, CuriosityUser>();
        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportDictionary => Exports;
        public Dictionary<Type, object> ActiveManagers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();

        public PluginManager()
        {
            Instance = this;

            async Task LoadTask()
            {
                await SetupConvars();

                DetachTickHandler(LoadTask);
                Load();
            }
            AttachTickHandler(LoadTask);
        }

        private async Task SetupConvars()
        {
            try
            {
                API.SetConvarServerInfo("Curiosity Missions", "v1.0.0.2550");

                IsDebugging = API.GetConvar("diagnostics_debug", "false") == "true";

                if (IsDebugging)
                {
                    Logger.Warn(string.Empty);
                    Logger.Warn($"----------------------------------------");
                    Logger.Warn($"-------- MISSION DEBUG ACTIVE ----------");
                    Logger.Warn($"----------------------------------------");
                    Logger.Warn(string.Empty);
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

                DiscordUrl = API.GetConvar("discord_url", "discord_url not set");
                API.SetConvarServerInfo("Discord", DiscordUrl);
            }
            catch (Exception ex)
            {
                ErrorText errorText = new ErrorText();
                errorText.PrintErrorText();
            }
        }

        private async void Load()
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

            foreach (var manager in ActiveManagers)
            {
                var method = manager.Key.GetMethod("Begin", BindingFlags.Public | BindingFlags.Instance);

                method?.Invoke(manager.Value, null);
            }

            var commands = new CommandFramework();
            // commands.Bind(typeof(DeveloperTools));

            Logger.Info($"[Managers] Successfully loaded in {loaded} manager(s)!");

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
            try
            {
                if (commandName.ToLower() == "missions")
                {
                    Logger.Info($"<- Mission Manager Start ->");
                    Logger.Info($"Active: {Managers.MissionManager.ActiveMissions.Count()}");
                    Logger.Info($"Assistance Requests: {Managers.MissionManager.ActiveMissions.Where(x => x.Value.AssistanceRequested).Count()}");
                    Logger.Info($"<- Mission Manager End ->");
                }

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

            ActiveManagers.Add(type, default(Type));

            var instance = Activator.CreateInstance(type);

            ActiveManagers[type] = instance;

            return instance;
        }

        public bool IsLoadingManager<T>() where T : Manager<T>, new()
        {
            return ActiveManagers.FirstOrDefault(self => self.Key == typeof(T)).Value is bool == false;
        }

        public object GetManager(Type type)
        {
            return ActiveManagers.FirstOrDefault(self => self.Key == type).Value;
        }

        public T GetManager<T>() where T : Manager<T>, new()
        {
            return (T)ActiveManagers.FirstOrDefault(self => self.Key == typeof(T)).Value;
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

        public async Task<string> RequestHttp(string url, string json, Dictionary<string, string> headers,
            string method = "GET")
        {
            var request = new Request();

            return (await request.Http(url, method, json, headers)).content;
        }
    }
}
