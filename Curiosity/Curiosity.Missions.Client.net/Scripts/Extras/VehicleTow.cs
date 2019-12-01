using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

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
        static bool IsServiceActive = false;

        // ENTITIES
        static Vehicle VehicleToRecover;

        // Coroner Stuff
        static Ped TowDriver;
        static Vehicle TowVehicle;

        static PedHash TowDriverHash = PedHash.WinClean01SMY;
        static VehicleHash TowVehicleHash = VehicleHash.Flatbed;

        static async Task OnVehicleExistsTask()
        {
            await Task.FromResult(0);
            if (Police.TrafficStop.TargetVehicle == null)
            {
                Reset();
                client.DeregisterTickHandler(OnVehicleExistsTask);
            }
        }

        static public async void RequestService()
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

                //if (!PoliceArrestVehicle.HasPedBeenPickedUp)
                //{
                //    Wrappers.Helpers.ShowNotification("Dispatch", "Suspect not Arrested", string.Empty, NotificationCharacter.CHAR_CALL911);
                //    return;
                //}

                int spawnDistance = Client.Random.Next(100, 200);

                VehicleToRecover = Game.PlayerPed.GetVehicleInFront();

                if (Police.TrafficStop.TargetVehicle != null && VehicleToRecover == null)
                    VehicleToRecover = Police.TrafficStop.TargetVehicle;

                if (VehicleToRecover != null)
                {
                    if (!VehicleToRecover.Exists())
                    {
                        Wrappers.Helpers.ShowNotification("City Impound", "What...", "Where the fuck did it go?", NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
                        Reset();
                        return;
                    }

                    Model towModel = TowDriverHash;
                    Model towVehicleModel = TowVehicleHash;

                    await towModel.Request(10000);
                    await towVehicleModel.Request(10000);

                    Vector3 closestVehicleNode = new Vector3();
                    Vector3 playerPosition = Game.PlayerPed.Position;
                    GetNthClosestVehicleNode(playerPosition.X, playerPosition.Y, playerPosition.Z, spawnDistance, ref closestVehicleNode, 0, 0, 0);

                    TowVehicle = await World.CreateVehicle(towVehicleModel, closestVehicleNode, 0f);
                    TowDriver = await TowVehicle.CreatePedOnSeat(VehicleSeat.Driver, towModel);

                    TowVehicle.IsInvincible = true;
                    TowVehicle.IsSirenActive = true;

                    TowDriver.IsInvincible = true;
                    TowDriver.IsPersistent = true;

                    TowDriver.Task.DriveTo(TowVehicle, playerPosition, 5f, 18f, (int)DrivingStyle.Normal);
                    TowVehicle.PlaceOnGround();
                    TowVehicle.IsPersistent = true;

                    TowVehicle.AttachBlip();
                    TowVehicle.AttachedBlip.Color = BlipColor.Blue;
                    TowVehicle.AttachedBlip.IsFlashing = true;

                    DistanceEta();

                    IsServiceActive = true;

                    bool isServiceEnroute = true;
                    bool isServiceClose = false;
                    bool isServiceOnScene = false;

                    while (isServiceEnroute)
                    {
                        await Client.Delay(100);
                        float currentDistance = TowVehicle.Position.Distance(VehicleToRecover.Position);

                        if (VehicleToRecover == null)
                            Reset();

                        if (currentDistance <= 180f && !isServiceClose)
                        {
                            Wrappers.Helpers.ShowNotification("City Impound", "Closing on your position", string.Empty, NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
                            isServiceClose = true;
                        }

                        if (currentDistance < 15f && !isServiceOnScene)
                        {
                            Wrappers.Helpers.ShowNotification("City Impound", "Is now on scene", string.Empty, NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
                            TaskVehicleTempAction(TowDriver.Handle, TowVehicle.Handle, 27, 2000);
                            TowVehicle.SetVehicleIndicators(true);
                            TowVehicle.IsSirenSilent = true;
                            TowDriver.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                            isServiceOnScene = true;
                        }

                        if (isServiceOnScene)
                        {
                            TowDriver.Task.GoTo(VehicleToRecover);
                            bool isTowDriverRunningToVehicle = true;

                            while (isTowDriverRunningToVehicle)
                            {
                                if (VehicleToRecover == null)
                                    Reset();

                                await Client.Delay(100);
                                currentDistance = TowDriver.Position.Distance(VehicleToRecover.Position);

                                if (currentDistance <= 5)
                                {
                                    isTowDriverRunningToVehicle = false;
                                    isServiceEnroute = false;

                                    CleanUpVehicle();
                                }
                                else
                                {
                                    TowDriver.Task.GoTo(VehicleToRecover);
                                    await Client.Delay(100);
                                }
                            }
                        }
                    }

                    
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
            AttachEntityToEntity(VehicleToRecover.Handle, TowVehicle.Handle, 20, -0.5f, -5.0f, 1.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 20, true);
            await Client.Delay(1000);
            TowDriver.DrivingStyle = DrivingStyle.Normal;
            TaskVehicleDriveWander(TowDriver.Handle, TowVehicle.Handle, 17.0f, (int)DrivingStyle.Normal);
            TowVehicle.IsSirenActive = true;
            await Client.Delay(10000);
            Reset(true);

            Police.TrafficStop.ResetVehicle();
        }

        static void DistanceEta()
        {
            float currentDistance = VehicleToRecover.Position.Distance(TowVehicle.Position);
            string etaMessage = string.Empty;
            switch (currentDistance)
            {
                case float n when (n < 100.0f):
                    etaMessage = "~g~1 mike";
                    break;
                case float n when (n < 300.0f):
                    etaMessage = "~g~2 mikes";
                    break;
                case float n when (n < 500.0f):
                    etaMessage = "~g~3 mikes";
                    break;
                case float n when (n > 500.0f):
                    etaMessage = "~g~4 mikes";
                    break;
            }
            Wrappers.Helpers.ShowNotification("City Impound", "I'm enroute", $"~b~ETA: ~w~{etaMessage}", NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
        }

        static public async void Reset(bool validCleanup = false)
        {
            if (validCleanup)
            {
                Wrappers.Helpers.ShowNotification("City Impound", "Now leaving...", $"", NotificationCharacter.CHAR_PROPERTY_TOWING_IMPOUND);
            }

            if (TowVehicle != null)
            {
                if (TowVehicle.Exists())
                {
                    if (TowVehicle.AttachedBlip != null)
                    {
                        if (TowVehicle.AttachedBlip.Exists())
                        {
                            TowVehicle.AttachedBlip.Delete();
                        }
                    }
                }
            }

            if (VehicleToRecover != null)
            {
                if (VehicleToRecover.Exists())
                {
                    if (VehicleToRecover.AttachedBlip != null)
                    {
                        if (VehicleToRecover.AttachedBlip.Exists())
                        {
                            VehicleToRecover.AttachedBlip.Delete();
                        }
                    }
                }
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

            if (VehicleToRecover != null)
            {
                if (VehicleToRecover.Exists())
                {
                    VehicleToRecover.MarkAsNoLongerNeeded();
                    VehicleToRecover.IsPersistent = false;
                    NetworkFadeOutEntity(VehicleToRecover.Handle, true, false);
                    await Client.Delay(5000);
                    VehicleToRecover.Delete();
                }
            }

            if (TowDriver != null)
            {
                if (TowDriver.Exists())
                {
                    TowDriver.MarkAsNoLongerNeeded();
                    TowDriver.IsPersistent = false;
                }
            }

            if (TowVehicle != null)
            {
                if (TowVehicle.Exists())
                {
                    TowVehicle.MarkAsNoLongerNeeded();
                    TowVehicle.IsPersistent = false;
                    NetworkFadeOutEntity(VehicleToRecover.Handle, true, false);
                    NetworkFadeOutEntity(TowDriver.Handle, true, false);
                    await Client.Delay(5000);
                    VehicleToRecover.Delete();
                    TowVehicle.Delete();
                }
            }
        }
    }
}
