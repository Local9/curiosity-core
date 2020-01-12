using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Atlas.Roleplay.Client.Commands;
using Atlas.Roleplay.Client.Commands.Impl;
using Atlas.Roleplay.Client.Diagnostics;
using Atlas.Roleplay.Client.Discord;
using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Events;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Events;
using CitizenFX.Core.UI;

#pragma warning disable 1998

namespace Atlas.Roleplay.Client
{
    public class AtlasPlugin : BaseScript
    {
        public static AtlasPlugin Instance { get; private set; }
        public static int MaximumPlayers { get; } = 32;

        public static int OnlinePlayers
        {
            get
            {
                var players = 0;

                for (var i = 0; i < MaximumPlayers; i++)
                {
                    if (API.NetworkIsPlayerActive(i)) players++;
                }

                return players;
            }
        }

        public readonly DiscordRichPresence DiscordRichPresence =
            new DiscordRichPresence(MaximumPlayers, "551099951497609217", "Society", "www.societyrp.se")
            {
                SmallAsset = "fivem",
                SmallAssetText = "https://fivem.net/",
                Status = "Loading..."
            };

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public AtlasPlayer Local { get; set; }
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();

        public AtlasPlugin()
        {
            Logger.Info("(Atlas): Constructor call from CitizenFX - BaseScript");

            Instance = this;

            API.DoScreenFadeOut(0);

#pragma warning disable 4014
            Load();
#pragma warning restore 4014
        }

        public async Task Load()
        {
            DiscordRichPresence.Commit();

            Logger.Info("[Managers] Loading in all managers, one moment please...");

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

            // Load event system first
            LoadManager(typeof(EventSystem));
            
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

            var commands = new CommandFramework();

            commands.Bind(typeof(Administrator));
            commands.Bind(typeof(DeveloperTools));
            commands.Bind(typeof(StatusCommands.Online));
            commands.Bind(typeof(RoleplayChat.Me));
            commands.Bind(typeof(RoleplayChat.Twitter));
            commands.Bind(typeof(UtilityTools.Reload));
            commands.Bind(typeof(UtilityTools.Report));

            API.SetMaxWantedLevel(0);

            AttachTickHandlers(this);

            Logger.Info("Load method has been completed.");
        }

        [TickHandler]
        private async Task OnTick()
        {
            Screen.Hud.HideComponentThisFrame(HudComponent.WeaponWheel);
            Screen.Hud.HideComponentThisFrame(HudComponent.Cash);
            Screen.Hud.HideComponentThisFrame(HudComponent.CashChange);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpCash);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpTagCashFromBank);
            Screen.Hud.HideComponentThisFrame(HudComponent.Saving);

            // Whitelist to make the reticle show. (Snipers, and certain weapons with scopes possibly)
            Screen.Hud.HideComponentThisFrame(HudComponent.Reticle);

            // Need to manage this based on server settings as some servers require police to be active

            API.SetDispatchCopsForPlayer(API.PlayerId(), false);

            for (var i = 1; i < 15; i++)
            {
                API.EnableDispatchService(i, false);
            }

            var position = API.GetEntityCoords(API.PlayerPedId(), false);

            API.ClearAreaOfCops(position.X, position.Y, position.Z, 150f, 0);
            API.SetPlayerWantedLevel(API.PlayerId(), 0, false);

            await Task.FromResult(0);
        }

        [TickHandler]
        private async Task SaveTask()
        {
            if (Local?.Character != null)
            {
                Local.Character.Metadata.LastPosition = Local.Entity.Position;
                Local.Character.Health = API.GetEntityHealth(Cache.Entity.Id);
                Local.Character.Shield = API.GetPedArmour(Cache.Entity.Id);
                Local.Character.Synchronize();
            }

            await Delay(5000);
        }

        [TickHandler]
        private async Task UserSynchronizeTask()
        {
            Local?.User?.PostUpdates();

            await Delay(10000);
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

        public void AttachTickHandlers(object instance)
        {
            TickHandlers.TryGetValue(instance.GetType(), out var methods);

            methods?.ForEach(async self =>
            {
                var handler = (TickHandler) self.GetCustomAttribute(typeof(TickHandler));

                if (handler.SessionWait)
                {
                    await Session.Loading();
                }

                Tick += (Func<Task>) Delegate.CreateDelegate(typeof(Func<Task>), instance, self);

                RegisteredTickHandlers.Add(instance.GetType());
            });
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
            return (T)  Managers.FirstOrDefault(self => self.Key == typeof(T)).Value;
        }

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }

        public void AttachNuiHandler(string pipe, EventCallback callback)
        {
            API.RegisterNuiCallbackType(pipe);

            EventHandlers[$"__cfx_nui:{pipe}"] += new Action<ExpandoObject, CallbackDelegate>((body, result) =>
            {
                var metadata = new EventMetadata();
                var properties = (IDictionary<string, object>) body;

                if (properties != null)
                {
                    foreach (var entry in properties)
                    {
                        if (!int.TryParse(entry.Key, out var index))
                        {
                            Logger.Info(
                                $"[Nui] [{pipe}] Payload `{entry.Key}` is not a number and therefore not written to.");

                            continue;
                        }

                        metadata.Write(index, entry.Value);
                    }
                }

                if (callback.GetType() == typeof(AsyncEventCallback))
                {
#pragma warning disable 4014
                    ((AsyncEventCallback) callback).AsyncTask(metadata);
#pragma warning restore 4014
                }
                else
                {
                    callback.Task(metadata);
                }
            });
        }
    }
}