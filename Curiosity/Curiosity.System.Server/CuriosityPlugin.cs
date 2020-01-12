using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.System.Library;
using Curiosity.System.Library.Events;
using Curiosity.System.Library.Models;
using Curiosity.System.Server.Diagnostics;
using Curiosity.System.Server.Events;
using Curiosity.System.Server.Web;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.System.Server
{
    public class CuriosityPlugin : BaseScript
    {
        const string CURIOSITY_VERSION = "v2.0.0.0001";

        public static CuriosityPlugin Instance { get; private set; }
        public static int MaximumPlayers { get; } = 32;
        public static int SaveInterval { get; } = 1000 * 60 * 3;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public EventHandlerDictionary EventRegistry => EventHandlers;
        public List<CuriosityUser> ActiveUsers { get; } = new List<CuriosityUser>();
        public long LastSave { get; set; } = Date.Timestamp;

        public CuriosityPlugin()
        {
            Instance = this;

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

            using (var context = new StorageContext())
            {
                Logger.Info(context.Database.Exists()
                    ? "Database `curiosity` exists."
                    : "Database `curiosity` doesn't exist, please create it first!");

                if (!context.Database.Exists()) return;

                async Task LoadTask()
                {
                    DetachTickHandler(LoadTask);
                    Load();
                }

                AttachTickHandler(LoadTask);
            }
        }

        private async void OnRconCommand(string commandName, List<object> args)
        {
            if (commandName.ToLower() != "save") return;

            foreach (var users in ActiveUsers)
            {
                await SaveOperation(users);
            }

            API.CancelEvent();
        }

        private async void Load()
        {
            Logger.Info("[Managers] Loading in all managers, one moment please...");

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

            AttachTickHandler(SaveTask);

            EventRegistry["rconCommand"] += new Action<string, List<object>>(OnRconCommand);
            EventRegistry["chat:global"] += new Action<ExpandoObject>(options =>
                new List<Player>(Players).ForEach(self => self.TriggerEvent("chat:addMessage", options)));

            EventSystem.GetModule().Attach("event:global", new EventCallback(metadata =>
            {
                foreach (var user in ActiveUsers)
                {
                    user.Send(metadata.Find<string>(0), metadata.AsEnumerable().Skip(1).ToArray());
                }

                return null;
            }));

            //EventSystem.GetModule().Attach("report:create", new AsyncEventCallback(async metadata =>
            //{
            //    var user = ActiveUsers.FirstOrDefault(self => self.Handle == metadata.Sender);

            //    if (user == null) return null;

            //    var id = TagId.Generate(5);
            //    var embeds = new JsonBuilder().Add("_", new Dictionary<string, object>
            //    {
            //        ["color"] = "32960",
            //        ["title"] = $"{metadata.Find<string>(0)} . {metadata.Find<string>(1)}",
            //        ["description"] = string.Join("\n",
            //            $"{metadata.Find<string>(2)}",
            //            "",
            //            $"● CitizenFX.Log [`View`](https://societyrp.se/reports/{id}/log)",
            //            $"● Report ID [`{id}`](https://societyrp.se/reports/{id})",
            //            $"● Användare `{user.Seed}`"
            //        ),
            //        ["footer"] = new Dictionary<string, object>()
            //        {
            //            ["text"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
            //        },
            //        ["url"] = "https://societyrp.se"
            //    });
            //    var data = new JsonBuilder().Add("content", "").Add("username", $"Report System | {id}")
            //        .Add("embeds", embeds.ToCollection()).Build();

            //    await RequestHttp(
            //        "https://discordapp.com/api/webhooks/551205130272374802/xMk9kdI3TpbEh-7pNSfUOICpYhjZDOfdQ5WC75Pop6kAFDwHrmpAOlgmDjZ7dwnRSPCA",
            //        data, new Dictionary<string, string>
            //        {
            //            ["Length"] = data.Length.ToString(),
            //            ["Content-Type"] = "application/json"
            //        });


            //    return null;
            //}));
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

        private async Task SaveTask()
        {
            if (LastSave + SaveInterval < Date.Timestamp)
            {
                if (ActiveUsers.Count > 0)
                {
                    Logger.Info("[Saves] Beginning `Save` operation on `Users` and `Characters`.");

                    foreach (var users in ActiveUsers)
                    {
                        await SaveOperation(users);
                    }

                    LastSave = Date.Timestamp;
                }

                await Delay(1000);
            }
        }

        public async Task SaveOperation(CuriosityUser user)
        {
            await user.Save();

            foreach (var character in user.Characters)
            {
                await character.Save();
            }
        }

    }
}
