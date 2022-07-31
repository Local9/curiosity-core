using CitizenFX.Core.UI;
using Curiosity.Core.Client.Commands;
using Curiosity.Core.Client.Commands.Impl;
using Curiosity.Core.Client.Discord;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Events;
using NativeUI;
using System.Linq;
using System.Reflection;

namespace Curiosity.Core.Client
{
    public class PluginManager : BaseScript
    {
        public const string DECOR_PED_OWNER = "PED_OWNER";

        public static MenuPool MenuPool;
        public static bool ProcessMouse = false;

        private bool IsAfkKickEnabled = false;
        private int MP_STUN_GUN;

        public static PluginManager Instance { get; private set; }
        public static Random Rand = new Random();

        public readonly DiscordRichPresence DiscordRichPresence =
            new DiscordRichPresence("590126930066407424", "LiveV Worlds", "forums.lifev.net")
            {
                SmallAsset = "fivem",
                SmallAssetText = "fivem.net",
                Status = "Connecting..."
            };

        public PlayerList PlayerList;
        public RelationshipGroup PlayerRelationshipGroup;
        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportDictionary => Exports;
        public CuriosityPlayer Local { get; set; }
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();
        public bool AfkCheckRunning { get; private set; }

        public MusicEvent ClientMusicEvent = new MusicEvent(eMusicEvents.AH1_START, eMusicEvents.AH1_STOP);

        DateTime lastTimePlayerStayedStill;
        Vector3 lastPosition;

        public PluginManager()
        {
            Logger.Info("[Curiosity]: Constructor Call from CitizenFX - BaseScript");

            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

            PlayerList = Players;

            PlayerRelationshipGroup = World.AddRelationshipGroup("PLAYER");

            Instance = this;

            API.DoScreenFadeOut(0);
            ClientMusicEvent = MusicEvents.LoadingEvents[Curiosity.Systems.Library.Utils.Utility.RANDOM.Next(MusicEvents.LoadingEvents.Count)];

            string mes = $"{ClientMusicEvent.Start}";

            API.PrepareMusicEvent(mes);
            API.PrepareMusicEvent($"{ClientMusicEvent.Stop}");
            API.PrepareMusicEvent($"{MusicEvents.DEFAULT_STOP}");

            API.SetAudioFlag("LoadMPData", true);
            API.TriggerMusicEvent(mes);

            API.DisplayPlayerNameTagsOnBlips(true);

            MenuPool = new MenuPool();

            Load();
        }

        public static async Task OnMenuDisplay()
        {
            try
            {
                if (!MenuPool.IsAnyMenuOpen()) return;

                MenuPool.ProcessMenus();
                if (ProcessMouse)
                    MenuPool.ProcessMouse();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "OnMenuDisplay");
            }
        }

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

        }

        private async void OnResourceStop(string resourceName)
        {
            Logger.Debug($"[STOPPING RESOURCE] {resourceName}");
            if (API.GetCurrentResourceName() != resourceName) return;

            await Cache.Character.Save();
        }

        private async Task Load()
        {
            DiscordRichPresence.Commit();

            AddTextEntry("G9_PAINT05", "Vice City"); // 0x03235520
            AddTextEntry("G9_PAINT13", "Kamen Rider"); // 0x06019DB0
            AddTextEntry("G9_PAINT14", "Chromatic Aberration"); // 0x0FAFB10C
            AddTextEntry("G9_PAINT15", "It's Christmas!"); // 0x1C9FCAEC
            AddTextEntry("G9_PAINT08", "Maisonette 9 Throwback"); // 0x2A6D23B3
            AddTextEntry("G9_PAINT02", "Night & Day"); // 0x2E6BABA4
            AddTextEntry("G9_PAINT16", "Temperature"); // 0x2E626E71
            AddTextEntry("G9_PAINT09", "Bubblegum"); // 0x3C1EC716
            AddTextEntry("G9_PAINT01", "Monochrome"); // 0x72A83420
            AddTextEntry("G9_PAINT03", "The Verlierer"); // 0x401A4F01
            AddTextEntry("G9_PAINT11", "Sunset"); // 0x5984C4B9
            AddTextEntry("G9_PAINT12", "The Seven"); // 0x8346183B
            AddTextEntry("G9_PAINT10", "Full Rainbow"); // 0x48712292
            AddTextEntry("G9_PAINT06", "Synthwave Nights"); // 0xBE32CB2C
            AddTextEntry("G9_PAINT07", "Four Seasons"); // 0xD07D6FC1
            AddTextEntry("G9_PAINT04", "Sprunk Extreme"); // 0xF175B1C5

            Vector5 position = await VehicleGenerationPositions.GetNearestParkingSpot();

            MP_STUN_GUN = API.GetHashKey("WEAPON_STUNGUN_MP");

            Logger.Info("[Curiosity]: Loading managers, please wait...");
            Logger.Info("[Curiosity]: Version: 2.0.1.4974");
            Logger.Info($"[Curiosity]: Client Time {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")}");

            try
            {

                List<MethodInfo> managers = Assembly.GetExecutingAssembly().GetExportedTypes()
                    .SelectMany(self => self.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                    .Where(self => self.GetCustomAttribute(typeof(TickHandler), false) != null).ToList();

                managers.ForEach(self =>
                    {
                        try
                        {
                            var type = self.DeclaringType;
                            if (type == null) return;

                            Logger.Debug($"[TickHandlers] {type.Name}::{self.Name}");

                            if (!TickHandlers.ContainsKey(type))
                            {
                                TickHandlers.Add(type, new List<MethodInfo>());
                            }

                            TickHandlers[type].Add(self);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"{ex}");
                            BaseScript.TriggerServerEvent("user:log:exception", $"Error with Tick; {ex.Message}", ex);
                        }
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
                API.DecorRegister("Player_Vehicle", 3);

                Logger.Info("Load method has been completed.");
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                BaseScript.TriggerServerEvent("user:log:exception", ex.Message, ex);
            }
        }

        //[TickHandler]
        //private async Task OnAfkKick()
        //{
        //    if (Local?.Character != null)
        //    {
        //        if (!Cache.Player.User.IsStaff && IsAfkKickEnabled)
        //        {
        //            if (lastPosition != (lastPosition = Cache.PlayerPed.Position))
        //                lastTimePlayerStayedStill = DateTime.Now;

        //            if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds >= 900)
        //            {
        //                EventSystem.GetModule().Send("user:kick:afk");
        //                return;
        //            }

        //            if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds == 780)
        //            {
        //                Notify.Custom("Time remaining before you're kicked for idling: 13m00s");
        //            }

        //            if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds == 600)
        //            {
        //                Notify.Custom("Time remaining before you're kicked for idling: 10m00s");
        //            }

        //            if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds == 300)
        //            {
        //                Notify.Custom("Time remaining before you're kicked for idling: 05m00s");
        //            }

        //            if (DateTime.Now.Subtract(lastTimePlayerStayedStill).TotalSeconds == 180)
        //            {
        //                Notify.Custom("Time remaining before you're kicked for idling: 02m00s");
        //            }
        //        }
        //    }

        //    await Delay(5000);
        //}

        [TickHandler]
        private async Task OnGeneralTick()
        {
            // Screen.Hud.HideComponentThisFrame(HudComponent.Reticle);
            // Screen.Hud.HideComponentThisFrame(HudComponent.WeaponWheel);

            int vehicleWeapon = -1553120962;
            API.SetWeaponDamageModifierThisFrame((uint)vehicleWeapon, 0.0f);
            API.SetWeaponDamageModifierThisFrame((uint)WeaponHash.StunGun, 0.0f);
            API.SetWeaponDamageModifierThisFrame((uint)MP_STUN_GUN, 0.0f);

            Screen.Hud.HideComponentThisFrame(HudComponent.Cash);
            Screen.Hud.HideComponentThisFrame(HudComponent.CashChange);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpCash);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpTagCashFromBank);
            Screen.Hud.HideComponentThisFrame(HudComponent.Saving);

            API.SetTextChatEnabled(false);
            API.DisablePlayerVehicleRewards(Game.Player.Handle);

            Game.Player.SetRunSpeedMultThisFrame(1f); // Speed hack to death            
        }

        DateTime nextTimeToSave = DateTime.UtcNow.AddSeconds(30);

        [TickHandler(SessionWait = true)]
        private async Task OnPeroidicSave()
        {
            if (DateTime.UtcNow < nextTimeToSave) return;

            if (Cache.Character.MarkedAsRegistered)
            {
                await Cache.Character.Save();
                await Delay(5000);
            }

            nextTimeToSave = DateTime.UtcNow.AddMinutes(1);
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
                try
                {
                    var handler = (TickHandler)self.GetCustomAttribute(typeof(TickHandler));

                    if (handler.SessionWait)
                    {
                        await Session.Loading();
                    }

                    Logger.Debug($"AttachTickHandlers -> {self.Name}");

                    Tick += (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), instance, self);

                    RegisteredTickHandlers.Add(instance.GetType());
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"AttachTickHandlers");
                }
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
            // Logger.Info($"[NUI]: {pipe}");
            API.RegisterNuiCallbackType(pipe);

            EventHandlers[$"__cfx_nui:{pipe}"] += new Action<IDictionary<string, object>, CallbackDelegate>(async (body, result) =>
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

                        if (entry.Value is null)
                        {
                            Logger.Error($"[Nui] [{pipe}]  Payload `{entry.Key}` value is null. Ending message.");
                            result(new { success = false });
                            return;
                        }

                        metadata.Write(index, entry.Value);
                    }
                }

                if (callback.GetType() == typeof(AsyncEventCallback))
                {
                    result(await ((AsyncEventCallback)callback).AsyncTask(metadata));
                }
                else
                {
                    result(callback.Task(metadata));
                }
            });
        }
    }
}
