using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Managers;
using Curiosity.Systems.Server.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server
{
    public class CuriosityPlugin : BaseScript
    {
        const string CURIOSITY_VERSION = "v2.0.0.0001";
        public static CuriosityPlugin Instance { get; private set; }
        public static PlayerList PlayersList { get; private set; }
        public static int MaximumPlayers { get; } = 32;
        public static int SaveInterval { get; } = 1000 * 60 * 3;
        public static bool IsDebugging { get; private set; }
        public List<CuriosityUser> ActiveUsers { get; } = new List<CuriosityUser>();
        public long LastSave { get; set; } = Date.Timestamp;

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();

        public CuriosityPlugin()
        {
            Logger.Info("[CuriosityPlugin] Loading...");
            
            Instance = this;

            PlayersList = Players;
            IsDebugging = API.GetConvar("diagnostics_debug", "false") == "true";

            if (IsDebugging)
            {
                Logger.Warn($"----------------------------------------");
                Logger.Warn($"------------ DEBUG ACTIVE --------------");
                Logger.Warn($"----------------------------------------");
            }

            API.SetConvarServerInfo("Discord", API.GetConvar("discord_url", "discord_url not set"));
            API.SetConvarServerInfo("Website", API.GetConvar("website_url", "website_url not set"));
            API.SetGameType(API.GetConvar("game_type", "game_type not set"));
            API.SetMapName("Life V - Curiosity Framework");

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

            async Task LoadTask()
            {
                DetachTickHandler(LoadTask);
                Load();
            }

            AttachTickHandler(LoadTask);
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

            Logger.Info($"[Managers] Successfully loaded in {loaded} manager(s)!");

            EventRegistry["rconCommand"] += new Action<string, List<object>>(OnRconCommand);
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
            Logger.Debug($"RCON Command: {commandName}");
            API.CancelEvent();
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
    }
}
