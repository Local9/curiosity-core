using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Missions.Client.net.Scripts.Extras
{
    class VehicleTow
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            RegisterCommand("tow", new Action(RequestService), false);
        }

        // STATE
        static public bool IsServiceActive = false;

        // ENTITIES
        static Vehicle vehToRemove;

        static async Task OnVehicleExistsTask()
        {
            await Task.FromResult(0);
            if (Police.TrafficStop.TargetVehicle == null)
            {
                Reset();
                client.DeregisterTickHandler(OnVehicleExistsTask);
            }
        }

        static public void RequestService()
        {
            try
            {
                Debug.WriteLine($"TOW-RequestService->Started");
                client.DeregisterTickHandler(OnVehicleExistsTask);

                if (IsServiceActive)
                {
                    Wrappers.Helpers.ShowNotification("City Impound", "Service is Unavailable", string.Empty, NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
                    return;
                }

                int spawnDistance = Client.Random.Next(100, 200);

                vehToRemove = Game.PlayerPed.GetVehicleInFront();

                if (Police.TrafficStop.TargetVehicle != null && vehToRemove == null)
                    vehToRemove = Police.TrafficStop.TargetVehicle;

                if (vehToRemove != null)
                {
                    if (vehToRemove.Driver != null)
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification($"~r~Driver must be removed");
                        return;
                    }

                    bool IsPlayerRelated = false;

                    if (vehToRemove.PreviouslyOwnedByPlayer) IsPlayerRelated = true;
                    if (vehToRemove == Client.CurrentVehicle) IsPlayerRelated = true;

                    if (DecorExistOn(vehToRemove.Handle, "Player_Vehicle"))
                    {
                        if (DecorGetBool(vehToRemove.Handle, "Player_Vehicle")) IsPlayerRelated = true;
                    }

                    if (IsPlayerRelated)
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification($"~r~Vehicle is owned by a Player.");
                        return;
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
            await Client.Delay(1000);
            string encodedString = Encode.StringToBase64($"{vehToRemove.NetworkId}");
            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Player:Vehicle:Delete", encodedString));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);

            Reset(true);
        }

        static public async void Reset(bool validCleanup = false)
        {
            if (validCleanup)
            {
                Wrappers.Helpers.ShowNotification("City Impound", "Vehicle Impounded", $"", NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
            }

            client.RegisterTickHandler(OnCooldownTask);
        }

        static async Task OnCooldownTask()
        {
            await Task.FromResult(0);
            int countdown = 60000;

            long gameTime = GetGameTimer();

            while ((GetGameTimer() - gameTime) < countdown)
            {
                await Client.Delay(500);
            }

            IsServiceActive = false;
            client.DeregisterTickHandler(OnCooldownTask);
            Wrappers.Helpers.ShowNotification("City Impound", "Impound Available", $"", NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
        }
    }
}
