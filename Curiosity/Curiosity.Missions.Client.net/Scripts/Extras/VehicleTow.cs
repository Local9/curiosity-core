﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Entity;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts.Extras
{
    class VehicleTow
    {
        static PluginManager PluginInstance => PluginManager.Instance;

        static public void Init()
        {
            RegisterCommand("tow", new Action(RequestService), false);
        }

        // STATE
        static public bool IsServiceActive = false;

        // ENTITIES
        static Vehicle vehToRemove;

        static public void RequestService()
        {
            try
            {
                Debug.WriteLine($"TOW-RequestService->Started");

                if (IsServiceActive)
                {
                    Wrappers.Helpers.ShowNotification("City Impound", "Service is Unavailable", string.Empty, NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
                    return;
                }

                vehToRemove = Game.PlayerPed.GetVehicleInFront(5f, 1f);

                if (vehToRemove != null)
                {
                    if (DecorGetInt(vehToRemove.Handle, PluginManager.DECOR_PLAYER_VEHICLE) > 0)
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification("~o~Player Vehicle");
                        return;
                    }

                    int tfVehHandle = 0;

                    if (DecorIsRegisteredAsType(PluginManager.DECOR_NPC_ACTIVE_TRAFFIC_STOP, 2))
                    {
                        tfVehHandle = DecorGetInt(vehToRemove.Handle, PluginManager.DECOR_TRAFFIC_STOP_VEHICLE_HANDLE);
                    }
                    else
                    {
                        Debug.WriteLine($"Decor is missing from the register");
                        return;
                    }

                    if (tfVehHandle == 0)
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification("~o~Vehicle was not found or wasn't in a traffic stop.");
                        return;
                    }

                    if (vehToRemove.Driver != null)
                    {
                        if (vehToRemove.Driver.Exists())
                        {
                            int pedId = vehToRemove.Driver.Handle;
                            RemovePedElegantly(ref pedId);
                        }
                    }

                    NetworkRequestControlOfEntity(vehToRemove.Handle);
                    CleanUpVehicle();
                }
                else
                {
                    Debug.WriteLine($"TOW-RequestService->VehicleNotFound");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TOW-RequestService-> {ex}");
                IsServiceActive = false;
            }
        }

        static async void CleanUpVehicle()
        {
            NetworkFadeOutEntity(vehToRemove.Handle, true, false);
            API.SetNetworkIdCanMigrate(vehToRemove.NetworkId, true);
            await PluginManager.Delay(1000);
            string encodedString = Encode.StringToBase64($"{vehToRemove.NetworkId}");
            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Player:Vehicle:Delete", encodedString));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);
            BaseScript.TriggerEvent("curiosity:interaction:vehicle:released", vehToRemove.NetworkId);

            if (vehToRemove.Exists())
            {
                vehToRemove.Delete();
                BaseScript.TriggerServerEvent("curiosity:Server:Missions:VehicleTowed");
            }

            Reset(true);
        }

        static public async void Reset(bool validCleanup = false)
        {
            if (validCleanup)
            {
                Wrappers.Helpers.ShowNotification("City Impound", "Vehicle Impounded", $"", NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
            }

            PluginInstance.RegisterTickHandler(OnCooldownTask);
        }

        static async Task OnCooldownTask()
        {
            await Task.FromResult(0);
            int countdown = 60000;

            long gameTime = GetGameTimer();

            while ((GetGameTimer() - gameTime) < countdown)
            {
                await PluginManager.Delay(500);
            }

            IsServiceActive = false;
            PluginInstance.DeregisterTickHandler(OnCooldownTask);
            Wrappers.Helpers.ShowNotification("City Impound", "Impound Available", $"", NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
        }
    }
}
