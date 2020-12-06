using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.ParkingMeters.Data;
using Curiosity.ParkingMeters.Models;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.ParkingMeters.Missions
{
    [MissionInfo("Parking Meter", "misParkingMeterTickets", 0f, 0f, 0f, MissionType.Mission, true, "None", PatrolZone.City)]
    public class ParkingMeterTickets : Mission
    {
        ParkingMeter parkingMeter;
        MissionState missionState;
        Vehicle vehicle;
        Blip missionBlip;

        System.Drawing.Color markerColor = System.Drawing.Color.FromArgb(255, 65, 105, 225);
        Vector3 scale = new Vector3(0.5f);

        public override void Start()
        {
            parkingMeter = ParkingMeterData.ParkingMetersCity[Utility.RANDOM.Next(ParkingMeterData.ParkingMetersCity.Count)];

            if (parkingMeter == null)
                Stop(EndState.Error);

            missionState = MissionState.Start;

            missionBlip = World.CreateBlip(parkingMeter.Position);
            missionBlip.Sprite = BlipSprite.PointOfInterest;
            missionBlip.ShowRoute = true;

            RegisterBlip(missionBlip);

            DiscordStatus($"Monitoring a Parking Meter");

            Notify.DispatchAI("Parking Meter Report", "Automated report of a parking meter running out, please ticket the vehicle.", iconType: 2);

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
                    vehicle.IsPersistent = true;
                    vehicle.IsPositionFrozen = true;
                    vehicle.IsInvincible = true;
                    vehicle.IsCollisionEnabled = false;
                    vehicle.IsRecordingCollisions = false;
                    
                    if (vehicle == null)
                    {
                        Stop(EndState.Error);
                        return;
                    }
                    
                    vehicle.Fx.LockStatus = VehicleLockStatus.Locked;
                    vehicle.Fx.IsEngineRunning = false;

                    RegisterVehicle(vehicle);

                    missionState = MissionState.TicketVehicle;

                    break;
                case MissionState.TicketVehicle:

                    Vector3 pos = missionBlip.Position;
                    pos.Z += 1f;

                    World.DrawMarker(MarkerType.UpsideDownCone, pos, Vector3.Zero, Vector3.Zero, scale, markerColor, bobUpAndDown: true);

                    if (Game.PlayerPed.Position.Distance(parkingMeter.Position) < 2f)
                    {
                        missionBlip.ShowRoute = false;

                        HelpMessage.CustomLooped(HelpMessage.Label.MISSION_PARKING_METER_CONTEXT);

                        if (Game.IsControlJustPressed(0, Control.Context))
                            missionState = MissionState.WritingTicket;
                    }

                    break;
                case MissionState.WritingTicket:

                    await Game.PlayerPed.AnimationTicket();

                    missionState = MissionState.Completion;

                    break;
                case MissionState.Completion:
                    missionState = MissionState.MissionEnded;
                    Pass();
                    break;
            }
        }

        internal enum MissionState
        {
            Start,
            SpawnVehicle,
            TicketVehicle,
            WritingTicket,
            Completion,
            MissionEnded
        }
    }
}
