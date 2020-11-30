using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Curiosity.TrafficViolations.MissionManager;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.TrafficViolations.Missions
{
    [MissionInfo("Insurance", "misTvInsurance", 0f, 0f, 0f, MissionType.Mission, true, "None", PatrolZone.Anywhere)]
    public class Insurance : Mission
    {
        private MissionState missionState;
        private Ped suspect;
        private Vehicle suspectVehicle;

        private bool hasTicketedPed = false;

        private bool isMissionStarted = false;
        private bool isMissionReady = false;

        List<VehicleHash> vehicleHashes = new List<VehicleHash>()
        {
            VehicleHash.Oracle2,
            VehicleHash.Panto,
            VehicleHash.Sandking,
            VehicleHash.SlamVan,
            VehicleHash.Adder,
            VehicleHash.Faggio,
            VehicleHash.Issi2,
            VehicleHash.Kuruma,
            VehicleHash.F620,
            VehicleHash.Dukes,
            VehicleHash.Baller,
            VehicleHash.Boxville,
            VehicleHash.Rumpo
        };

        List<PedHash> pedHashes = new List<PedHash>()
        {
            PedHash.Tourist01AFM,
            PedHash.Tourist01AFY,
            PedHash.Lost01GFY,
            PedHash.Lost01GMY,
        };

        public async override void Start()
        {
            missionState = MissionState.Started;

            suspectVehicle = await Vehicle.Spawn(vehicleHashes.Random(),
                Players[0].Character.Position.AroundStreet(200f, 400f));

            if (suspectVehicle == null)
            {
                Stop(EndState.Error);
                return;
            }

            Mission.RegisterVehicle(suspectVehicle);

            suspect = await Ped.Spawn(pedHashes.Random(), suspectVehicle.Position, sidewalk: true);

            if (suspect == null)
            {
                Stop(EndState.Error);
                return;
            }

            suspect.IsImportant = true;
            suspect.IsMission = true;
            suspect.IsSuspect = true;

            Mission.RegisterPed(suspect);

            suspect.PutInVehicle(suspectVehicle);
            suspect.Task.CruiseWithVehicle(suspectVehicle.Fx, float.MaxValue,
                (int)Collections.CombinedVehicleDrivingFlags.Normal);

            Blip b = suspectVehicle.AttachBlip();
            b.Color = BlipColor.Red;
            b.ShowRoute = true;

            suspectVehicle.IsSpikable = true;
            suspectVehicle.IsMission = true;
            suspectVehicle.IsTowable = true;

            Notify.Dispatch("~o~CODE 1", $"~y~Traffic Monitoring: Unlicensed~s~~n~~b~Make~s~: {suspectVehicle.Name}~n~~b~Color~s~: {suspectVehicle.Fx.Mods.PrimaryColor}~n~~b~Plate~s~: {suspectVehicle.Fx.Mods.LicensePlate}~n~~g~GPS Updated");

            isMissionReady = true;

            DiscordStatus("Searching for a Vehicle");

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            while (!isMissionReady)
            {
                await BaseScript.Delay(100);
            }

            if (suspect.Position.Distance(Game.PlayerPed.Position) < 400f && !isMissionStarted)
            {
                isMissionStarted = true;
                missionState = MissionState.SeakingVehicle;
            }

            if (suspect.Position.Distance(Game.PlayerPed.Position) > 600f && isMissionStarted && NumberPedsArrested == 0)
            {
                missionState = MissionState.Escaped;
            }

            if (suspect != null && suspect.IsDead)
                missionState = MissionState.SuspectDied;


            switch (missionState)
            {
                case MissionState.Started:
                    break;
                case MissionState.SeakingVehicle:
                    if (suspectVehicle.IsInRangeOf(Game.PlayerPed.Position, 10f))
                    {
                        HelpMessage.CustomLooped(HelpMessage.Label.MISSION_STOP_VEHICLE);

                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            suspect.Task.ParkVehicle(suspectVehicle.Fx, suspectVehicle.Position, Game.PlayerPed.Heading);
                            missionState = MissionState.VehicleParking;
                        }
                    }
                    break;
                case MissionState.VehicleParking:
                    if (suspectVehicle.Fx.Speed < 4.0f && Game.PlayerPed.IsInVehicle())
                    {
                        HelpMessage.CustomLooped(HelpMessage.Label.MISSION_VEHICLE_STOPPED);
                    }
                    else
                    {
                        missionState = MissionState.TicketVehicle;
                    }
                    break;
                case MissionState.TicketVehicle:
                    if (suspect.IsInRangeOf(Game.PlayerPed.Position, 4f) && !Game.PlayerPed.IsInVehicle())
                    {
                        HelpMessage.CustomLooped(HelpMessage.Label.MISSION_VEHICLE_TICKET_DRIVER);
                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            await AnimationHandler.AnimationClipboard();

                            missionState = MissionState.Passed;
                        }
                    }
                    break;
                case MissionState.Escaped:
                    Fail("Suspect no longer visible....");
                    break;
                case MissionState.SuspectDied:
                    Fail("Suspect dead.");
                    break;
                case MissionState.Passed:
                    Pass();
                    break;
            }
        }
    }
}
