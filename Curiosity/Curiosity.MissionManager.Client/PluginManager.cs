using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Commands;
using Curiosity.MissionManager.Client.Commands.Impl;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Discord;
using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        internal static List<Blip> Blips = new List<Blip>();
        private DateTime LastDiscordPresenceUpdate = DateTime.Now;

        public static PlayerList PlayerList;

        public CuriosityPlayer Local { get; set; }
        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportRegistry => Exports;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();
        public static int MaximumPlayers { get; } = 32;
        public static bool IsSpecialDay { get; internal set; } = false;
        public static bool IsPlayerSpawned { get; internal set; } = false;

        public readonly DiscordRichPresence DiscordRichPresence =
            new DiscordRichPresence(MaximumPlayers, "590126930066407424", "banner", "forums.lifev.net")
            {
                SmallAsset = "fivem",
                SmallAssetText = "fivem.net",
                Status = "Connecting..."
            };

        public PluginManager()
        {
            Logger.Info("[Curiosity]: Constructor Call from CitizenFX - BaseScript");

            PlayerList = Players;

            Instance = this;

            EventHandlers["onResourceStop"] += new Action<string>(OnClientResourceStop);
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerSpawned"] += new Action<string>(OnPlayerSpawned);

            Load();
        }

        private async Task Load()
        {
            Logger.Info("[Curiosity]: Loading managers, please wait...");

            DiscordRichPresence.Commit();

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

            Logger.Info($"--------------------------------------------------");

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

            Logger.Info($"--------------------------------------------------");

            Logger.Info($"[Managers] Successfully loaded in {loaded} manager(s)!");

            var commands = new CommandFramework();

            commands.Bind(typeof(DeveloperTools));
            commands.Bind(typeof(PlayerCommands));

            AttachTickHandlers(this);

            Logger.Info("Load method has been completed.");
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            if (!API.HasAnimDictLoaded("veh@low@front_ps@enter_exit"))
                API.RequestAnimDict("veh@low@front_ps@enter_exit");

            if (!API.HasAnimDictLoaded("rcmnigel3_trunk"))
                API.RequestAnimDict("rcmnigel3_trunk");

            if (!API.HasAnimDictLoaded("rcmepsilonism8"))
                API.RequestAnimDict("rcmepsilonism8");

            Notify.Info($"~b~Mission Resource ~g~Started");
        }

        private void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Notify.Info($"~b~Mission Resource ~r~Stopped");

            if (Mission.isOnMission)
                Mission.currentMission.Stop(EndState.Restart);

            if (JobManager.IsOfficer)
                BaseScript.TriggerEvent("curiosity:Client:Interface:Duty", false, false, "unemployed");

            foreach (Blip blip in Blips) blip.Delete();
        }
        private void OnPlayerSpawned(dynamic obj)
        {
            IsPlayerSpawned = true;
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
                var handler = (TickHandler)self.GetCustomAttribute(typeof(TickHandler));

                if (handler.SessionWait)
                {
                    await Session.Loading();
                }

                Tick += (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), instance, self);

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
            return (T)Managers.FirstOrDefault(self => self.Key == typeof(T)).Value;
        }

        public void AttachTickHandler(Func<Task> task)
        {
            // Logger.Debug($"Attaching {task.Method.Name}");
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            // Logger.Debug($"Detaching {task.Method.Name}");
            Tick -= task;
        }

        public void AttachNuiHandler(string pipe, EventCallback callback)
        {
            API.RegisterNuiCallbackType(pipe);

            EventHandlers[$"__cfx_nui:{pipe}"] += new Action<ExpandoObject, CallbackDelegate>((body, result) =>
            {
                var metadata = new EventMetadata();
                var properties = (IDictionary<string, object>)body;

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
                    ((AsyncEventCallback)callback).AsyncTask(metadata);
                }
                else
                {
                    callback.Task(metadata);
                }
            });
        }

        [TickHandler]
        private async Task OnDiscordPresenceUpdateTick()
        {
            await Session.Loading();

            if (DateTime.Now.Subtract(LastDiscordPresenceUpdate).TotalSeconds >= 90)
            {
                DiscordRichPresence.Commit();

                LastDiscordPresenceUpdate = DateTime.Now;
            }

            await BaseScript.Delay(1500);
        }
    }
}
