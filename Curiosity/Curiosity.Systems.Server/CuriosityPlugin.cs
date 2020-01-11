using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Server.Web;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server
{
    public class CuriosityPlugin : BaseScript
    {
        public static CuriosityPlugin Instance { get; private set; }
        public static PlayerList PlayerList { get; private set; }
        public static int MaximumPlayers { get; } = 32;
        public static int SaveInterval { get; } = 1000 * 60 * 3;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public EventHandlerDictionary EventRegistry => EventHandlers;
        public List<CuriosityUser> ActiveUsers { get; } = new List<CuriosityUser>();
        public long LastSave { get; set; } = Date.Timestamp;

        public CuriosityPlugin()
        {
            Instance = this;
            PlayerList = Players;

            // We don't require steam
            API.SetConvar("sv_authMaxVariance", "");
            API.SetConvar("sv_authMinTrust", "");

            API.SetConvarServerInfo("Discord", API.GetConvar("discord_url", "discord_url not set"));
            API.SetConvarServerInfo("Website", API.GetConvar("website_url", "website_url not set"));
            API.SetGameType(API.GetConvar("game_type", "game_type not set"));
            API.SetMapName(API.GetConvar("map_name", "map_name not set"));
        }

        public async Task<string> RequestHttp(string url, string json, Dictionary<string, string> headers,
            string method = "GET")
        {
            var request = new Request();

            return (await request.Http(url, method, json, headers)).content;
        }

        public CuriosityUser Lookup(int handle)
        {
            return ActiveUsers.FirstOrDefault(self => self.Handle == handle);
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

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }
    }
}
