using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Helpers;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Missions.Client.net.Extensions;

namespace Curiosity.Missions.Client.net.Scripts.Police
{
    class TrafficStop
    {
        static Client client = Client.GetInstance();

        static Vehicle TrafficStopVehicle;
        static Blip TrafficStopVehicleBlip;

        static bool AttemptedTrafficStop = false;
        static bool HasTrafficStoppedVehicleParked = false;

        static int VehicleHandle;

        public static void Setup()
        {
            VehicleHandle = Game.PlayerPed.CurrentVehicle.Handle;

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                client.RegisterTickHandler(OnTrafficStopTask);
                Screen.ShowNotification("~b~Traffic Stops~s~: ~g~Enabled");
            }
        }

        public static void Dispose()
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                client.DeregisterTickHandler(OnTrafficStopTask);
                Screen.ShowNotification("~b~Traffic Stops~s~: ~r~Disabled");
            }
        }

        static async Task OnTrafficStopTask()
        {
            await Task.FromResult(0);
            try
            {
                if (TrafficStopVehicle != null && !AttemptedTrafficStop)
                {
                    Game.DisableControlThisFrame(2, Control.VehicleNextRadio);
                    Game.DisableControlThisFrame(2, Control.VehiclePrevRadio);
                    Game.DisableControlThisFrame(2, Control.VehicleRadioWheel);

                    if (Game.IsDisabledControlPressed(2, Control.VehicleRadioWheel))
                    {
                        if (TrafficStopVehicleBlip.Exists())
                            TrafficStopVehicleBlip.Delete();

                        TrafficStopVehicle.Driver.Task.ClearAllImmediately();
                        TrafficStopVehicle.Driver.MarkAsNoLongerNeeded();
                        TrafficStopVehicle.MarkAsNoLongerNeeded();

                        TrafficStopVehicle = null;

                        while (Game.IsDisabledControlPressed(2, Control.VehicleRadioWheel))
                        {
                            await Client.Delay(0);

                            Game.DisableControlThisFrame(2, Control.VehicleNextRadio);
                            Game.DisableControlThisFrame(2, Control.VehiclePrevRadio);
                            Game.DisableControlThisFrame(2, Control.VehicleRadioWheel);
                        }
                    }
                    else if (Game.PlayerPed.Position.Distance(TrafficStopVehicle.Position) > 50f)
                    {
                        if (TrafficStopVehicleBlip.Exists())
                            TrafficStopVehicleBlip.Delete();

                        TrafficStopVehicle = null;
                    }
                    else if (Game.IsControlJustPressed(0, Control.Pickup))
                    {
                        AttemptedTrafficStop = true;
                        Screen.DisplayHelpTextThisFrame($"Initiated Traffic Stop.");

                        API.BlipSiren(Game.PlayerPed.CurrentVehicle.Handle);

                        client.RegisterTickHandler(TrafficStopInitiated);
                        client.DeregisterTickHandler(OnTrafficStopTask);
                    }
                    else
                    {
                        Screen.DisplayHelpTextThisFrame($"Vehicle in front has been ~b~blipped~s~.~n~Press ~INPUT_PICKUP~ to start a ~b~Traffic Stop~s~~n~Press ~INPUT_COVER~ to ~b~Release");
                    }
                }
                else if (TrafficStopVehicle != null && AttemptedTrafficStop)
                {
                    if (Game.PlayerPed.Position.Distance(TrafficStopVehicle.Position) > 50f)
                    {
                        if (TrafficStopVehicleBlip.Exists())
                            TrafficStopVehicleBlip.Delete();

                        TrafficStopVehicle = null;
                        AttemptedTrafficStop = false;
                    }
                }
                else
                {
                    if (!Game.PlayerPed.IsInVehicle()) return;

                    Entity entity = WorldExt.GetEntityInFrontOfPlayer(10f);

                    if (entity == null) return;

                    long gameTime = API.GetGameTimer();

                    while ((API.GetGameTimer() - gameTime) < 5000)
                    {
                        await Client.Delay(0);
                    }

                    Entity entityCheck = WorldExt.GetEntityInFrontOfPlayer(10f);

                    if (entityCheck != entity) return;

                    if (entity.Model.IsVehicle)
                    {
                        TrafficStopVehicle = entity as Vehicle;
                        TrafficStopVehicleBlip = TrafficStopVehicle.AttachBlip();
                        TrafficStopVehicleBlip.Sprite = BlipSprite.Standard;
                        TrafficStopVehicleBlip.Color = BlipColor.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                {
                    Debug.WriteLine($"{ex}");
                }
            }
        }

        static async Task TrafficStopInitiated()
        {
            try
            {

                Game.DisableControlThisFrame(2, Control.VehicleNextRadio);
                Game.DisableControlThisFrame(2, Control.VehiclePrevRadio);
                Game.DisableControlThisFrame(2, Control.VehicleRadioWheel);

                Ped driver = TrafficStopVehicle.Driver;
                driver.BlockPermanentEvents = true;

                if (Client.Random.Next(5) == 1 && !HasTrafficStoppedVehicleParked)
                {
                    driver.IsPositionFrozen = false;
                    TrafficStopVehicle.IsPositionFrozen = false;

                    driver.Task.FleeFrom(Game.PlayerPed);

                    client.DeregisterTickHandler(TrafficStopInitiated);
                }
                else
                {
                    if (!HasTrafficStoppedVehicleParked)
                    {
                        driver.IsPositionFrozen = false;
                        TrafficStopVehicle.IsPositionFrozen = false;

                        Vector3 offset = new Vector3(3f, 10f, 0f);
                        Vector3 pulloverLocation = TrafficStopVehicle.GetOffsetPosition(offset);

                        driver.Task.ParkVehicle(TrafficStopVehicle, pulloverLocation, Game.PlayerPed.CurrentVehicle.Heading);
                        await Client.Delay(500);

                        HasTrafficStoppedVehicleParked = true;
                    }

                    if (TrafficStopVehicle.IsStopped)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                            Screen.DisplayHelpTextThisFrame($"Vehicle has stopped.~n~Press ~INPUT_PICKUP~ to ~b~move the suspect.~s~~n~Press ~INPUT_COVER~ to ~b~release.~n~~s~Walk up the the driver to talk.");

                        TrafficStopVehicle.IsPositionFrozen = true;
                    }

                    if (Game.IsControlJustPressed(0, Control.Pickup) && Game.PlayerPed.IsInVehicle())
                    {
                        HasTrafficStoppedVehicleParked = false;
                    }

                    if (Game.IsDisabledControlPressed(2, Control.VehicleRadioWheel) && Game.PlayerPed.IsInVehicle())
                    {
                        Game.DisableControlThisFrame(2, Control.VehicleNextRadio);
                        Game.DisableControlThisFrame(2, Control.VehiclePrevRadio);
                        Game.DisableControlThisFrame(2, Control.VehicleRadioWheel);

                        Reset();
                    }

                    if (!Game.PlayerPed.IsInVehicle() && Game.PlayerPed.Position.Distance(driver.Position) < 2.5f)
                    {
                        if (Client.Random.Next(10) == 1)
                        {
                            driver.IsPositionFrozen = false;
                            TrafficStopVehicle.IsPositionFrozen = false;

                            driver.RelationshipGroup = Static.Relationships.HostileRelationship;

                            driver.Weapons.Give(WeaponHash.Pistol, 500, true, true);
                            driver.Task.ShootAt(Game.PlayerPed);

                            await Client.Delay(3000);

                            driver.Task.FightAgainstHatedTargets(30f);
                            driver.Task.FleeFrom(Game.PlayerPed);

                            client.DeregisterTickHandler(TrafficStopInitiated);
                        }
                    }
                }

                if (Game.PlayerPed.Position.Distance(TrafficStopVehicle.Position) > 250f)
                {
                    Reset();
                }

            }
            catch (Exception ex)
            {
                client.DeregisterTickHandler(TrafficStopInitiated);
                client.RegisterTickHandler(OnTrafficStopTask);
            }
        }

        static void Reset()
        {
            if (TrafficStopVehicleBlip.Exists())
                TrafficStopVehicleBlip.Delete();

            TrafficStopVehicle.IsPositionFrozen = false;
            TrafficStopVehicle.Driver.IsPositionFrozen = false;

            TrafficStopVehicle.Driver.Task.ClearAll();
            TrafficStopVehicle.Driver.MarkAsNoLongerNeeded();
            TrafficStopVehicle.MarkAsNoLongerNeeded();

            TrafficStopVehicle = null;

            client.DeregisterTickHandler(TrafficStopInitiated);
            client.RegisterTickHandler(OnTrafficStopTask);
        }
    }
}
