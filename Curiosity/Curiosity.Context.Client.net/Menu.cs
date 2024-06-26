﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Helper;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Context.Client.net
{
    class Crosshair
    {
        public bool crosshair = false;
    }

    class MenuSetting
    {
        public dynamic menu;
        public bool showDutyMenu;
        public bool speeding;
        public string dutyMenuText;
        public int idEntity;
    }

    public class Menu : BaseScript
    {
        private static Menu _instance;

        bool showMenu = false;
        bool showDutyMenu = false;
        bool playingEmote = false;
        bool toggleCarboot = false;
        bool isChatInputActive = false;
        bool isCinematicMode = false;

        string dutyMenuText;
        string dutyEventToCall;

        bool trafficStopActive = false;

        public Menu()
        {
            _instance = this;

            RegisterEventHandler("curiosity:Client:Chat:ChatboxActive", new Action<bool>(OnChatboxActive));
            RegisterEventHandler("curiosity:Player:UI:CinematicMode", new Action(OnCinematicMode));

            RegisterEventHandler("curiosity:Client:Context:ShowDutyMenu", new Action<bool, string, string>(OnDutyMenu));
            RegisterEventHandler("curiosity:Client:Context:TrafficStopActive", new Action<bool>(OnTrafficStopActive));

            RegisterNuiEventHandler("disablenuifocus", new Action<dynamic>(DisableNuiFocus));
            RegisterNuiEventHandler("togglelock", new Action<dynamic>(OnToggleLockStatus));
            RegisterNuiEventHandler("togglecarboot", new Action<dynamic>(OnOpenCarboot));
            RegisterNuiEventHandler("openDutyMenu", new Action<dynamic>(OnOpenDutyMenu));

            RegisterNuiEventHandler("vpoTicket", new Action<dynamic>(OnVpoTicket));
            RegisterNuiEventHandler("vpoSpeedingTicket", new Action<dynamic>(OnVpoSpeedingTicket));
            RegisterNuiEventHandler("vpoRelease", new Action<dynamic>(OnVpoRelease));

            RegisterTickHandler(OnTick);
        }

        void OnTrafficStopActive(bool state)
        {
            trafficStopActive = state;
        }

        void OnCinematicMode()
        {
            isCinematicMode = !isCinematicMode;
            isChatInputActive = isCinematicMode;
        }

        void OnVpoSpeedingTicket(dynamic nui)
        {
            TriggerEvent("curiosity:Client:Police:ReleaseSpeedingTicketedAI");
            DisableNuiFocus(null);
        }

        void OnVpoTicket(dynamic nui)
        {
            TriggerEvent("curiosity:Client:Police:ReleaseTicketedAI");
            DisableNuiFocus(null);
        }

        void OnVpoRelease(dynamic nui)
        {
            TriggerEvent("curiosity:Client:Police:ReleaseAI");
            DisableNuiFocus(null);
        }

        void OnDutyMenu(bool enable, string menuText, string eventToCall)
        {
            showDutyMenu = enable;
            dutyMenuText = menuText;
            dutyEventToCall = eventToCall;
        }

        void OnChatboxActive(bool isChatActive)
        {
            isChatInputActive = isChatActive;
        }

        void OnOpenCarboot(dynamic nui)
        {
            TriggerEvent("curiosity:Client:Menu:Carboot", nui.id);
        }

        void OnToggleLockStatus(dynamic nui)
        {
            TriggerEvent("curiosity:Client:Menu:CarLock", nui.id);
        }

        void OnOpenDutyMenu(dynamic nui)
        {
            if (!string.IsNullOrEmpty(dutyEventToCall))
                TriggerEvent(dutyEventToCall);

            DisableNuiFocus(null);
        }

        void Crosshair(bool toggle)
        {
            SendNuiMessage(JsonConvert.SerializeObject(new Crosshair { crosshair = toggle }));
        }

        void DisableNuiFocus(dynamic nui)
        {
            showMenu = false;
            SetNuiFocus(false, false);
            Crosshair(false);
            SendNuiMessage(JsonConvert.SerializeObject(new MenuSetting { menu = false }));
        }

        async Task OnTick()
        {
            try
            {
                if (trafficStopActive)
                {
                    Crosshair(false);

                    if (showMenu)
                        SendNuiMessage(JsonConvert.SerializeObject(new MenuSetting { menu = false }));

                    showMenu = false;

                    return;
                }

                Entity playerPed = Game.PlayerPed;

                if (Game.PlayerPed.IsAiming || Game.PlayerPed.IsAimingFromCover)
                {
                    Crosshair(false);
                }
                else if (!Game.PlayerPed.IsInVehicle())
                {
                    if (!isChatInputActive)
                    {
                        Entity ent = GetEntityInCrosshair(playerPed, playerPed);

                        if (ent == null)
                        {
                            if (showMenu)
                            {
                                DisableNuiFocus(null);
                            }
                            Crosshair(false);

                            await Task.FromResult(0);
                            return;
                        }

                        int entityType = GetEntityType(ent.Handle);

                        if (entityType == 2) // Vehicle
                        {
                            if (!showMenu)
                            {
                                SetNuiFocus(false, false);
                            }
                            Crosshair(true);

                            if (ControlHelper.IsControlJustReleased(Control.Context))
                            {
                                showMenu = true;
                                SetNuiFocus(true, true);

                                Vehicle vehicle = (Vehicle)ent;

                                if (API.DecorGetBool(vehicle.Handle, "curiosity:police:IsPulledOver"))
                                {
                                    SendNuiMessage(JsonConvert.SerializeObject(new MenuSetting
                                    {
                                        menu = "vehiclePulledOver",
                                        speeding = API.DecorGetBool(vehicle.Handle, "curiosity:police:Speeding"),
                                        idEntity = ent.Handle
                                    })); ;
                                }
                                else
                                {
                                    SendNuiMessage(JsonConvert.SerializeObject(new MenuSetting
                                    {
                                        menu = "vehicle",
                                        showDutyMenu = (vehicle.ClassType == VehicleClass.Emergency && showDutyMenu),
                                        dutyMenuText = dutyMenuText,
                                        idEntity = ent.Handle
                                    }));
                                }
                            }
                        }
                        //else if (entityType == 1) // Ped
                        //{
                        //    if (!showMenu)
                        //    {
                        //        SetNuiFocus(false, false);

                        //        if (ent.IsPositionFrozen)
                        //            FreezeEntityPosition(ent.Handle, false);
                        //    }
                        //    Crosshair(true);

                        //    if (ControlHelper.IsControlJustReleased(Control.Context))
                        //    {
                        //        showMenu = true;
                        //        SetNuiFocus(true, true);
                        //        SendNuiMessage(JsonConvert.SerializeObject(new MenuSetting { menu = "user", idEntity = ent.Handle }));

                        //        FreezeEntityPosition(ent.Handle, true);
                        //        Ped ped = (Ped)ent;
                        //        ped.Task.ClearAll();
                        //        ped.Task.LookAt(playerPed, 3000);
                        //    }
                        //}
                        else
                        {
                            SetNuiFocus(false, false);
                            Crosshair(false);
                            SendNuiMessage(JsonConvert.SerializeObject(new MenuSetting { menu = false }));
                            showMenu = false;
                        }
                    }

                    if (playingEmote)
                    {
                        if (ControlHelper.IsControlJustPressed(Control.Context))
                        {
                            ClearPedTasks(playerPed.Handle);
                            playingEmote = false;
                        }
                    }
                }
                else
                {
                    if (showMenu)
                    {
                        SetNuiFocus(false, false);
                        Crosshair(false);
                        SendNuiMessage(JsonConvert.SerializeObject(new MenuSetting { menu = false }));
                        showMenu = false;
                    }
                    else
                    {
                        Crosshair(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            await Task.FromResult(0);
        }

        /// <summary>
        /// Registers a NUI/CEF event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        void RegisterNuiEventHandler(string name, Delegate action)
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
        /// Registers a network event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        void RegisterEventHandler(string name, Delegate action)
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
        /// Registers a tick function
        /// </summary>
        /// <param name="action"></param>
        void RegisterTickHandler(Func<Task> action)
        {
            try
            {
                // Debug.WriteLine($"{action.Method.Name}");
                Tick += action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }



        Entity GetEntityInCrosshair(Entity source, Entity ignore, float distance = 5f)
        {
            try
            {
                RaycastResult raycast = World.Raycast(source.Position, CitizenFX.Core.Game.PlayerPed.Position + 10 * GameplayCamForwardVector(), IntersectOptions.Everything, ignore);
                if (raycast.DitHitEntity)
                {
                    return raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Log.Info($"[MENU - WORLDPROBE] GetEntityInFrontOfPlayer Error: {ex.Message}");
            }
            return default(Entity);
        }

        static Vector3 GameplayCamForwardVector()
        {
            try
            {
                Vector3 rotation = (float)(Math.PI / 180.0) * Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 2);
                return Vector3.Normalize(new Vector3((float)-Math.Sin(rotation.Z) * (float)Math.Abs(Math.Cos(rotation.X)), (float)Math.Cos(rotation.Z) * (float)Math.Abs(Math.Cos(rotation.X)), (float)Math.Sin(rotation.X)));
            }
            catch (Exception ex)
            {
                Log.Error($"WorldProbe GameplayCamForwardVector Error: {ex.Message}");
            }
            return default(Vector3);
        }
    }
}
