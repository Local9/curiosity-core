﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using Curiosity.Client.net.Classes.Environment.UI;
using Curiosity.Shared.Client.net.Models;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Client.net
{
    public class Client : BaseScript
    {
        public static int PedHandle { get { return Game.PlayerPed.Handle; } }
        public static int PlayerHandle { get { return Game.Player.Handle; } }
        public static GlobalEntity.User User;
        public static Vehicle CurrentVehicle = null;
        public static Vehicle ownedVehicle = null;

        public static bool isSessionActive = false;

        public static PlayerList players;

        private static Client _instance;

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

            ClassLoader.Init();
            RegisterTickHandler(OnTick);
            RegisterEventHandler("curiosity:Client:Player:SessionActivated", new Action(OnSessionActive));

            Log.Info("Curiosity.Client.net loaded\n");

            //RegisterEventHandler("TriggerEventNearPoint", new Action<string>(HandleLocalEvent));
            //Client.GetInstance().PointEventHandlers["Communication.LocalChat"] = new Func<PointEvent, Task>(HandleLocalChat);
        }

        async void OnSessionActive()
        {
            BaseScript.TriggerServerEvent("curiosity:Server:Character:RoleCheck");
            await Delay(1000);
            isSessionActive = true;
        }

        /// <summary>
        /// Default/main tick function
        /// </summary>
        /// <returns></returns>
        public async Task OnTick()
        {
            try
            {
                // UI.Render();
                await UpdateFrameSettings();
                if (Game.PlayerPed.IsInVehicle())
                {
                    if (Classes.Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                    {
                        if (CurrentVehicle != Game.PlayerPed.CurrentVehicle)
                        {
                            CurrentVehicle = Game.PlayerPed.CurrentVehicle;
                        }
                    }
                    else if (CurrentVehicle != Game.PlayerPed.CurrentVehicle)
                    {
                        if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                        {
                            CurrentVehicle = Game.PlayerPed.CurrentVehicle;
   
                            if (!CurrentVehicle.PreviouslyOwnedByPlayer)
                                API.SetVehicleExclusiveDriver(CurrentVehicle.Handle, Client.PedHandle);
                        }
                    }
                }
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
        public void RegisterEventHandler(string name, Delegate action)
        {
            try
            {
                EventHandlers[name] += action;
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
                Function.Call(Hash.REGISTER_NUI_CALLBACK_TYPE, name);
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
                Debug.WriteLine($"{action.Method.Name}");
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
