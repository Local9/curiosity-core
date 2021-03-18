using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Commands;
using Curiosity.Core.Client.Commands.Impl;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Discord;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Curiosity.Core.Client
{
    public class PluginManager : BaseScript
    {
        public const string DECOR_PED_OWNER = "PED_OWNER";

        private bool IsAfkKickEnabled = false;

        public static PluginManager Instance { get; private set; }
        public static Random Rand = new Random();
        public static int MaximumPlayers { get; } = 32;

        public readonly DiscordRichPresence DiscordRichPresence =
            new DiscordRichPresence(MaximumPlayers, "590126930066407424", "Live V", "forums.lifev.net")
            {
                SmallAsset = "fivem",
                SmallAssetText = "fivem.net",
                Status = "Connecting..."
            };

        public PlayerList PlayerList;

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportDictionary => Exports;
        public CuriosityPlayer Local { get; set; }
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();
        public bool AfkCheckRunning { get; private set; }

        DateTime lastTimePlayerStayedStill;
        Vector3 lastPosition;

        public PluginManager()
        {
            Logger.Info("[Curiosity]: Constructor Call from CitizenFX - BaseScript");

            PlayerList = Players;

            Instance = this;

            API.DoScreenFadeOut(0);

            Load();
        }

        private async Task Load()
        {
            DiscordRichPresence.Commit();

            Logger.Info("[Curiosity]: Loading managers, please wait...");

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

            commands.Bind(typeof(DeveloperTools));
            commands.Bind(typeof(PlayerCommands));
            commands.Bind(typeof(StaffCommands));

            AttachTickHandlers(this);

            API.DecorRegister(DECOR_PED_OWNER, 3);

            Logger.Info("Load method has been completed.");
        }

        [TickHandler]
        private async Task SaveTask()
        {
            if (Local?.Character != null)
            {
                if (Local.Character.MarkedAsRegistered)
                {
                    Local.Character.LastPosition = Local.Entity.Position;
                    Local.Character.Health = Cache.PlayerPed.Health;
                    Local.Character.Armor = Cache.PlayerPed.Armor;
                    Local.Character.Save();
                }

                if (!Cache.Player.User.IsStaff && IsAfkKickEnabled)
                {
                    if (lastPosition != (lastPosition = Cache.PlayerPed.Position))
                        lastTimePlayerStayedStill = DateTime.Now;

                    if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds >= 900)
                    {
                        EventSystem.GetModule().Send("user:kick:afk");
                        return;
                    }

                    if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds == 780)
                    {
                        Notify.Custom("Time remaining before you're kicked for idling: 13m00s");
                    }

                    if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds == 600)
                    {
                        Notify.Custom("Time remaining before you're kicked for idling: 10m00s");
                    }

                    if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds == 300)
                    {
                        Notify.Custom("Time remaining before you're kicked for idling: 05m00s");
                    }

                    if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds == 180)
                    {
                        Notify.Custom("Time remaining before you're kicked for idling: 02m00s");
                    }
                }
            }

            await Delay(5000);
        }

        [TickHandler]
        private async Task OnTick()
        {
            // Screen.Hud.HideComponentThisFrame(HudComponent.Reticle);
            // Screen.Hud.HideComponentThisFrame(HudComponent.WeaponWheel);

            Screen.Hud.HideComponentThisFrame(HudComponent.Cash);
            Screen.Hud.HideComponentThisFrame(HudComponent.CashChange);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpCash);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpTagCashFromBank);
            Screen.Hud.HideComponentThisFrame(HudComponent.Saving);

            API.SetTextChatEnabled(false);
            API.DisablePlayerVehicleRewards(Game.Player.Handle);

            // Disable wanted levels
            API.ClearPlayerWantedLevel(Game.Player.Handle);
            API.SetMaxWantedLevel(0);
            API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
            API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
            API.SetPlayerWantedLevelNoDrop(Game.Player.Handle, 0, false);
            Game.Player.WantedLevel = 0;

            Game.Player.SetRunSpeedMultThisFrame(1f); // Speed hack to death            

            await Task.FromResult(0);
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
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }

        public void AttachNuiHandler(string pipe, EventCallback callback)
        {
            API.RegisterNuiCallbackType(pipe);

            EventHandlers[$"__cfx_nui:{pipe}"] += new Action<IDictionary<string, object>, CallbackDelegate>((body, result) =>
            {
                var metadata = new EventMetadata();

                if (body != null)
                {
                    foreach (var entry in body)
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

                result(new { ok = true });
            });
        }
    }
}
