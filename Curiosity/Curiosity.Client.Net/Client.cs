using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.Environment.UI;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Client.net
{
    public class Client : BaseScript
    {
        public static int PedHandle { get { return Game.PlayerPed.Handle; } }
        public static int PlayerHandle { get { return Game.Player.Handle; } }

        public static bool ShowPlayerNames = true;
        public static bool StaffShowPlayerNames = false;

        public static GlobalEntity.User User;
        public static string PLAYER_GROUP = "PLAYER";
        private const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";
        public static RelationshipGroup PlayerRelationshipGroup;
        public static int minutesAfkKick = 15;

        public static CitizenFX.Core.Vehicle CurrentVehicle;

        public static bool isSessionActive = false;

        public static PlayerList players;

        private static Client _instance;

        static long GameTimeNow;

        public static Client GetInstance()
        {
            return _instance;
        }

        private const float DefaultPlayerSpeed = 1f;

        public Dictionary<string, Func<PointEvent, Task>> PointEventHandlers = new Dictionary<string, Func<PointEvent, Task>>();

        public Client()
        {
            _instance = this;

            players = Players;
            PlayerRelationshipGroup = World.AddRelationshipGroup(PLAYER_GROUP);

            API.DecorRegister("DEATH", 3);

            ClassLoader.Init();
            RegisterTickHandler(OnHideHudTick);
            RegisterEventHandler("curiosity:Client:Player:SessionActivated", new Action<bool, bool, int>(OnSessionActive));
            RegisterEventHandler("curiosity:Player:Menu:VehicleId", new Action<int>(OnVehicleId));

            //RegisterEventHandler("TriggerEventNearPoint", new Action<string>(HandleLocalEvent));
            //Client.GetInstance().PointEventHandlers["Communication.LocalChat"] = new Func<PointEvent, Task>(HandleLocalChat);
        }

        public static void OnVehicleId(int vehicleId)
        {
            if (CurrentVehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                CurrentVehicle = new Vehicle(vehicleId);
            }
            else if (Client.CurrentVehicle.Handle != vehicleId)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                CurrentVehicle = new Vehicle(vehicleId);
            }
        }

        async void OnSessionActive(bool showBlips, bool showLocation, int afkMinutes)
        {
            if (showBlips)
                RegisterTickHandler(PlayerBlips.OnTickShowPlayerBlips);

            if (showLocation)
                RegisterTickHandler(Location.OnShowStreetNameTick);

            minutesAfkKick = afkMinutes;

            BaseScript.TriggerServerEvent("curiosity:Server:Character:RoleCheck");
            await Delay(1000);
            isSessionActive = true;

            RegisterTickHandler(OnSessionCheck);
        }

        public async Task OnSessionCheck()
        {
            GameTimeNow = API.GetGameTimer();
            while ((API.GetGameTimer() - GameTimeNow) > 60000)
            {
                GameTimeNow = API.GetGameTimer();
                Client.TriggerServerEvent("curiosity:Server:Session:Ping");
            }
        }

        /// <summary>
        /// Default/main tick function
        /// </summary>
        /// <returns></returns>
        public async Task OnHideHudTick()
        {
            try
            {
                // UI.Render();
                await UpdateFrameSettings();
            }
            catch (Exception ex)
            {
                Log.Error($"[CurrentVehicle] {ex.Message}");
            }

            await Task.FromResult(0);
        }

        public async Task UpdateFrameSettings()
        {
            try
            {
                Game.Player.SetRunSpeedMultThisFrame(DefaultPlayerSpeed);
                CitizenFX.Core.UI.Screen.Hud.HideComponentThisFrame(CitizenFX.Core.UI.HudComponent.VehicleName);
                CitizenFX.Core.UI.Screen.Hud.HideComponentThisFrame(CitizenFX.Core.UI.HudComponent.AreaName);
                CitizenFX.Core.UI.Screen.Hud.HideComponentThisFrame(CitizenFX.Core.UI.HudComponent.StreetName);
                API.DisplayCash(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            await Task.FromResult(0);
        }

        /// <summary>
        /// Registers a network event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterCommand(string command, Delegate action, bool restricted)
        {
            try
            {
                API.RegisterCommand(command, action, restricted);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Registers a network event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterEventHandler(string name, Delegate action)
        {
            try
            {
                EventHandlers[$"{name}"] += action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Registers a NUI/CEF event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterNuiEventHandler(string name, Delegate action)
        {
            try
            {
                API.RegisterNuiCallbackType(name);
                RegisterEventHandler(string.Concat("__cfx_nui:", name), action);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Registers a tick function
        /// </summary>
        /// <param name="action"></param>
        public void RegisterTickHandler(Func<Task> action)
        {
            try
            {
                Tick += action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Removes a tick function from the registry
        /// </summary>
        /// <param name="action"></param>
        public void DeregisterTickHandler(Func<Task> action)
        {
            try
            {
                Tick -= action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Registers an export, functions to be used by other resources. Registered exports still have to be defined in the __resource.lua file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterExport(string name, Delegate action)
        {
            Exports.Add(name, action);
        }

        //public void HandleLocalEvent(string serializedPointEvent)
        //{
        //    try
        //    {
        //        PointEvent pointEvent = Helpers.MsgPack.Deserialize<PointEvent>(serializedPointEvent);

        //        if (pointEvent.IgnoreOwnEvent && pointEvent.SourceServerId == Game.Player.ServerId)
        //            return;

        //        if (Game.PlayerPed.Position.DistanceToSquared2D(pointEvent.ToVector3()) < Math.Pow(pointEvent.AoeRange, 2))
        //            PointEventHandlers[pointEvent.EventName].Invoke(pointEvent);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex.Message);
        //    }
        //}

        //private Task HandleLocalChat(PointEvent pointEvent)
        //{
        //    TriggerEvent("Chat.Message", Client.players[pointEvent.SourceServerId].Name, "#FFD23F", pointEvent.SerializedArguments);

        //    return Task.FromResult(0);
        //}
    }
}
