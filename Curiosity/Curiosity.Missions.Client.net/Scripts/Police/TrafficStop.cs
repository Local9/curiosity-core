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

        public static void Setup()
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                client.RegisterTickHandler(OnTrafficStopTask);
        }

        public static void Dispose()
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                client.DeregisterTickHandler(OnTrafficStopTask);
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
    }
}
