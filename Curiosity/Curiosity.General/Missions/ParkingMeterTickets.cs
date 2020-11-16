using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.ParkingMeters.Data;
using Curiosity.ParkingMeters.Models;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.ParkingMeters.Missions
{
    [MissionInfo("City Parking Meters", "misPmCity", 0f, 0f, 0f, MissionType.Mission, true, "None")]
    public class ParkingMeterTickets : Mission
    {
        ParkingMeter parkingMeter;
        MissionState missionState;
        Vehicle vehicle;

        public override void Start()
        {
            switch(PatrolZone)
            {
                case PatrolZone.City:
                    parkingMeter = ParkingMeterData.ParkingMetersCity[Utility.RANDOM.Next(ParkingMeterData.ParkingMetersCity.Count)];
                    break;
                case PatrolZone.County:
                    parkingMeter = ParkingMeterData.ParkingMetersCounty[Utility.RANDOM.Next(ParkingMeterData.ParkingMetersCounty.Count)];
                    break;
                default:
                    Stop(EndState.Error); // Unknown Patrol Zone
                    break;
            }

            if (parkingMeter == null)
                Stop(EndState.Error);

            missionState = MissionState.Start;

            Blip blip = World.CreateBlip(parkingMeter.Position);
            blip.Sprite = BlipSprite.PointOfInterest;

            RegisterBlip(blip);

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            switch(missionState)
            {
                case MissionState.Start:
                    if (Game.PlayerPed.Position.Distance(parkingMeter.Position) < 100f)
                        missionState = MissionState.SpawnVehicle;
                    break;
                case MissionState.SpawnVehicle:
                    vehicle = await Vehicle.Spawn(parkingMeter.ParkingMeterVehicle.Vehicle, parkingMeter.ParkingMeterVehicle.Position, parkingMeter.ParkingMeterVehicle.Heading, false, false, true);
                    
                    if (vehicle == null)
                    {
                        Stop(EndState.Error);
                        return;
                    }
                    
                    vehicle.Fx.LockStatus = VehicleLockStatus.Locked;
                    vehicle.Fx.IsEngineRunning = false;

                    break;
                case MissionState.TicketVehicle:

                    if (Game.PlayerPed.Position.Distance(parkingMeter.Position) < 2f)
                    {
                        HelpMessage.CustomLooped(HelpMessage.Label.MISSION_CLERK_SPEAK_WITH);

                        if (Game.IsControlJustPressed(0, Control.Context))
                            missionState = MissionState.Completion;
                    }

                    break;
                case MissionState.Completion:
                    Pass();
                    break;
            }
        }

        internal enum MissionState
        {
            Start,
            SpawnVehicle,
            TicketVehicle,
            Completion
        }
    }
}
