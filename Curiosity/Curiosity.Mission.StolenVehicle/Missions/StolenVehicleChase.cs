using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.StolenVehicle.Missions
{
    [MissionInfo("Stolen Vehicle (1 Suspect)", "misSvChase", 0f, 0f, 0f, MissionType.StolenVehicle, true, "None", chanceOfSpawn: .8f)]
    public class StolenVehicleChase : Mission
    {
        private MissionState missionState;
        private Ped criminal;
        private Vehicle stolenVehicle;

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

        List<WeaponHash> weaponHashes = new List<WeaponHash>()
        {
            WeaponHash.Pistol,
            WeaponHash.SMG,
            WeaponHash.MicroSMG
        };

        public async override void Start()
        {
            missionState = MissionState.Started;

            stolenVehicle = await Vehicle.Spawn(vehicleHashes.Random(),
                Players[0].Character.Position.AroundStreet(200f, 400f));

            if (stolenVehicle == null)
            {
                Stop(EndState.Error);
                return;
            }

            Mission.RegisterVehicle(stolenVehicle);

            criminal = await Ped.Spawn(pedHashes.Random(), stolenVehicle.Position, sidewalk: true);

            if (criminal == null)
            {
                Stop(EndState.Error);
                return;
            }

            criminal.IsImportant = true;
            criminal.IsMission = true;
            criminal.IsSuspect = true;

            if (Utility.RANDOM.Bool(0.8f))
            {
                criminal.Fx.RelationshipGroup = (uint)Collections.RelationshipHash.Prisoner;
                criminal.Fx.RelationshipGroup.SetRelationshipBetweenGroups(Game.PlayerPed.RelationshipGroup, Relationship.Hate);
                criminal.Fx.Weapons.Give(weaponHashes.Random(), 20, true, true);
            }

            Mission.RegisterPed(criminal);

            criminal.PutInVehicle(stolenVehicle);
            criminal.Task.CruiseWithVehicle(stolenVehicle.Fx, float.MaxValue,
                (int)Collections.CombinedVehicleDrivingFlags.Fleeing);

            Blip b = stolenVehicle.AttachBlip();
            b.Color = BlipColor.Red;
            b.ShowRoute = true;

            stolenVehicle.IsSpikable = true;
            stolenVehicle.IsMission = true;
            stolenVehicle.IsTowable = true;

            Notify.Dispatch("~o~CODE 3", $"~y~Stolen Vehicle~s~~n~~b~Make~s~: {stolenVehicle.Name}~n~~b~Plate~s~: {stolenVehicle.Fx.Mods.LicensePlate}~n~~g~GPS Updated");

            isMissionReady = true;

            DiscordStatus("Chasing a Stolen Vehicle");

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            if (criminal != null)
            {
                Fail("Failed to capture and arrest the perps");
            }

            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            while(!isMissionReady)
            {
                await BaseScript.Delay(100);
            }

            float roll = API.GetEntityRoll(stolenVehicle.Fx.Handle);
            if ((roll > 75.0f || roll < -75.0f) && stolenVehicle.Fx.Speed < 4f)
            {
                TaskFleeVehicle(criminal);
            }

            if (stolenVehicle.Fx.Speed < 4.0f && criminal.IsInVehicle)
            {
                long gameTimer = API.GetGameTimer();
                Vector3 location = stolenVehicle.Position;
                while (criminal.IsInVehicle && stolenVehicle.Position.Distance(location) < 4.0f && stolenVehicle.Position.Distance(Game.PlayerPed.Position) < 10f)
                {
                    await BaseScript.Delay(100);
                    if ((API.GetGameTimer() - gameTimer) > 20000)
                    {
                        TaskFleeVehicle(criminal);
                    }
                }
            }

            if (stolenVehicle.Health < 200)
            {
                TaskFleeVehicle(criminal);
            }

            if (!criminal.IsKneeling && !criminal.IsInVehicle && !criminal.IsHandcuffed)
            {
                TaskFleeVehicle(criminal);
            }

            if (criminal.Position.Distance(Game.PlayerPed.Position) < 400f && !isMissionStarted)
            {
                isMissionStarted = true;
            }

            if (criminal.Position.Distance(Game.PlayerPed.Position) > 600f && isMissionStarted && NumberPedsArrested == 0)
            {
                missionState = MissionState.Escaped;
            }

            if (NumberPedsArrested > 0)
                missionState = MissionState.End;

            if (criminal.IsDead)
                missionState = MissionState.SuspectDied;

            switch (missionState)
            {
                case MissionState.Started:
                    break;
                case MissionState.Escaped:
                    Fail("Vehicle got away....");
                    break;
                case MissionState.SuspectDied:
                    Fail("Suspect dead.");
                    break;
                case MissionState.End:
                    Pass();
                    break;
            }
        }

        void TaskFleeVehicle(Ped ped)
        {
            if (ped == null) return;
            if (ped.IsFleeing) return;

            Blip b = ped.AttachBlip();
            b.Color = BlipColor.Red;
            b.ShowRoute = false;
            b.Scale = .5f;
            b.Sprite = BlipSprite.Enemy;

            ped.IsFleeing = true;

            TaskSequence taskSequence = new TaskSequence();

            if (ped.IsInVehicle)
                taskSequence.AddTask.LeaveVehicle(LeaveVehicleFlags.BailOut);

            taskSequence.AddTask.FleeFrom(Game.PlayerPed);
            ped.Task.PerformSequence(taskSequence);
            taskSequence.Close();

            DiscordStatus("Chasing a Suspect on Foot");
        }
    }
}
