using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.Wrappers;

namespace Curiosity.Missions.Client.net.Scripts.Extras
{
    class Coroner
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            RegisterCommand("ems", new Action(RequestService), false);
        }

        // STATE
        static public bool IsServiceActive = false;
        
        // ENTITIES
        static Ped PedToRecover;

        // Coroner Stuff
        static Ped CoronerDriver;
        static Vehicle CoronerVehicle;

        static PedHash CoronerDriverHash = PedHash.Doctor01SMM;
        static VehicleHash CoronerVehicleHash = VehicleHash.Speedo;

        static public async void RequestService()
        {
            try
            {
                Helpers.AnimationRadio();

                if (IsServiceActive)
                {
                    Wrappers.Helpers.ShowNotification("Coroner", "Service Unavailable", string.Empty);
                    return;
                }

                int spawnDistance = Client.Random.Next(100, 200);
                RaycastResult raycastResult = World.RaycastCapsule(Game.PlayerPed.Position, Game.PlayerPed.Position, 2.0f, IntersectOptions.Peds1, Game.Player.Character);
                if (raycastResult.DitHitEntity)
                {
                    if (raycastResult.HitEntity.Model.IsPed)
                    {
                        PedToRecover = raycastResult.HitEntity as Ped;

                        if (PedToRecover.IsAlive)
                        {
                            Wrappers.Helpers.ShowNotification("Coroner", "Really...", "We don't pick up the living.");
                            Reset();
                            return;
                        }

                        if (!PedToRecover.Exists())
                        {
                            Wrappers.Helpers.ShowNotification("Coroner", "Ped not found.", string.Empty);
                            Reset();
                            return;
                        }

                        Client.TriggerEvent("curiosity:interaction:coroner", PedToRecover.NetworkId); // PED UPDATE

                        Model coronerModel = CoronerDriverHash;
                        Model coronerVehicleModel = CoronerVehicleHash;

                        await coronerModel.Request(10000);
                        await coronerVehicleModel.Request(10000);

                        Vector3 closestVehicleNode = new Vector3();
                        Vector3 playerPosition = Game.PlayerPed.Position;
                        GetNthClosestVehicleNode(playerPosition.X, playerPosition.Y, playerPosition.Z, spawnDistance, ref closestVehicleNode, 0, 0, 0);

                        CoronerVehicle = await World.CreateVehicle(coronerVehicleModel, closestVehicleNode, 0f);
                        CoronerDriver = await CoronerVehicle.CreatePedOnSeat(VehicleSeat.Driver, coronerModel);

                        CoronerVehicle.IsInvincible = true;
                        CoronerVehicle.IsSirenActive = true;

                        CoronerDriver.IsInvincible = true;
                        CoronerDriver.IsPersistent = true;

                        CoronerDriver.Task.DriveTo(CoronerVehicle, playerPosition, 5f, 18f, (int)DrivingStyle.Rushed);
                        CoronerVehicle.PlaceOnGround();
                        CoronerVehicle.IsPersistent = true;

                        CoronerVehicle.AttachBlip();
                        CoronerVehicle.AttachedBlip.Color = BlipColor.Blue;
                        CoronerVehicle.AttachedBlip.IsFlashing = true;

                        DistanceEta();

                        IsServiceActive = true;

                        bool isServiceEnroute = true;
                        bool isServiceClose = false;
                        bool isServiceOnScene = false;

                        while (isServiceEnroute)
                        {
                            await Client.Delay(100);
                            float currentDistance = PedToRecover.Position.Distance(CoronerVehicle.Position);

                            if (currentDistance <= 180f && !isServiceClose)
                            {
                                Wrappers.Helpers.ShowNotification("Dispatch", "Coroner Update", $"Is nearing your location.");
                                isServiceClose = true;
                            }

                            if (currentDistance < 15f && !isServiceOnScene)
                            {
                                Wrappers.Helpers.ShowNotification("Dispatch", "Coroner Update", $"Is on scene.");
                                TaskVehicleTempAction(CoronerDriver.Handle, CoronerVehicle.Handle, 27, 2000);
                                CoronerVehicle.SetVehicleIndicators(true);
                                CoronerVehicle.IsSirenSilent = true;
                                CoronerDriver.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                                isServiceOnScene = true;
                            }

                            if (isServiceOnScene)
                            {
                                CoronerDriver.Task.GoTo(PedToRecover);
                                bool isCoronerRunningToPed = true;
                                
                                while (isCoronerRunningToPed)
                                {
                                    await Client.Delay(100);
                                    currentDistance = PedToRecover.Position.Distance(CoronerDriver.Position);
                                    
                                    if (currentDistance <= 5)
                                    {
                                        isCoronerRunningToPed = false;
                                        isServiceEnroute = false;
                                    }
                                }
                            }
                        }

                        CleanUpPed();
                    }
                }
            }
            catch (Exception ex)
            {
                IsServiceActive = false;
            }
        }

        static async void CleanUpPed()
        {
            string animationDict = "amb@medic@standing@kneel@enter";
            RequestAnimDict(animationDict);
            while (!HasAnimDictLoaded(animationDict))
            {
                await Client.Delay(0);
            }

            CoronerDriver.Task.PlayAnimation(animationDict, "enter");

            await Client.Delay(10000);

            if (PedToRecover.IsPlayer)
            {
                Wrappers.Helpers.ShowNotification("Dispatch", "Coroner Update", $"Sorry, this one is lost.");
            }
            else
            {
                PedToRecover.Delete();
            }

            CoronerDriver.Task.ClearAnimation(animationDict, "enter");

            await Client.Delay(300);

            RemoveAnimDict(animationDict);

            await Client.Delay(500);

            CoronerDriver.Task.EnterVehicle(CoronerVehicle, VehicleSeat.Driver);

            await Client.Delay(5500);

            Reset(true);
        }

        static void DistanceEta()
        {
            float currentDistance = PedToRecover.Position.Distance(CoronerVehicle.Position);
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
            Wrappers.Helpers.ShowNotification("Dispatch", "Coroner Enroute", $"~b~ETA:~w~ {etaMessage}");
        }

        static public async void Reset(bool validCleanup = false)
        {
            if (PedToRecover != null)
            {
                if (PedToRecover.Exists())
                {
                    PedToRecover.MarkAsNoLongerNeeded();
                    PedToRecover.IsPersistent = false;
                }
            }

            if (CoronerDriver != null)
            {
                if (CoronerDriver.Exists())
                {
                    CoronerDriver.MarkAsNoLongerNeeded();
                    CoronerDriver.IsPersistent = false;
                }
            }

            if (CoronerVehicle != null)
            {
                if (CoronerVehicle.Exists())
                {
                    if (CoronerVehicle.AttachedBlip != null)
                    {
                        if (CoronerVehicle.AttachedBlip.Exists())
                        {
                            CoronerVehicle.AttachedBlip.Delete();
                        }
                    }

                    CoronerVehicle.MarkAsNoLongerNeeded();
                    CoronerVehicle.IsPersistent = false;
                }
            }

            if (validCleanup)
            {
                Wrappers.Helpers.ShowNotification("Dispatch", "Coroner Leaving", $"");
            }

            int countdownCounter = 60;

            while (countdownCounter > 0)
            {
                countdownCounter--;
                await Client.Delay(1000);
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
            Wrappers.Helpers.ShowNotification("Dispatch", "Coroner Available", $"");
        }
    }
}
