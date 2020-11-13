using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.StolenVehicle.Missions
{
    [MissionInfo("Stolen Vehicle (2 Suspects)", "misSvChase2", 0f, 0f, 0f, MissionType.StolenVehicle, true, "None", chanceOfSpawn: .5f)]
    public class StolenVehicleChaseTwo : Mission
    {
        private MissionState missionState;
        private Ped criminal;
        private Ped criminalPassenger;
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
            criminal.AttachBlip(BlipColor.Red, true);


            await BaseScript.Delay(100);

            criminalPassenger = await Ped.Spawn(pedHashes.Random(), stolenVehicle.Position, sidewalk: true);

            if (criminalPassenger != null)
            {
                criminalPassenger.IsImportant = true;
                criminalPassenger.IsMission = true;
                criminalPassenger.IsSuspect = true;
                
                criminalPassenger.Fx.DropsWeaponsOnDeath = false;
                criminalPassenger.Fx.RelationshipGroup = (uint)Collections.RelationshipHash.Prisoner;
                criminalPassenger.Fx.RelationshipGroup.SetRelationshipBetweenGroups(Game.PlayerPed.RelationshipGroup, Relationship.Hate);
                criminalPassenger.Fx.Weapons.Give(weaponHashes.Random(), 20, true, true);

                API.SetPedCombatAttributes(criminalPassenger.Handle, 2, true);
                API.SetPedCombatAttributes(criminalPassenger.Handle, 46, true);

                API.RegisterHatedTargetsAroundPed(criminalPassenger.Handle, 50f);
                criminalPassenger.Task.FightAgainstHatedTargets(50f);

                criminalPassenger.AttachBlip(BlipColor.Red, false);

                Mission.RegisterPed(criminalPassenger);

                criminalPassenger.PutInVehicle(stolenVehicle, VehicleSeat.Any);
            }

            stolenVehicle.IsSpikable = true;
            stolenVehicle.IsMission = true;
            stolenVehicle.IsTowable = true;

            UiTools.Dispatch("~r~CODE 3~s~: Stolen Vehicle - ARMED", $"~b~Make~s~: {stolenVehicle.Name}~n~~b~Plate~s~: {stolenVehicle.Fx.Mods.LicensePlate}~n~~g~GPS Updated");

            isMissionReady = true;

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            if (criminal != null && criminalPassenger != null)
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

                if (criminalPassenger != null)
                    TaskFleeVehicle(criminalPassenger);
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

                        if (criminalPassenger != null)
                            TaskFleeVehicle(criminalPassenger);
                    }
                }
            }

            if (stolenVehicle.Health < 200)
            {
                TaskFleeVehicle(criminal);

                if (criminalPassenger != null)
                    TaskFleeVehicle(criminalPassenger);
            }

            if (!criminal.IsKneeling && !criminal.IsInVehicle && !criminal.IsHandcuffed)
            {
                TaskFleeVehicle(criminal);
            }


            if (criminalPassenger != null)
            {
                if (!criminalPassenger.IsKneeling && !criminalPassenger.IsInVehicle && !criminalPassenger.IsHandcuffed)
                {
                    TaskFleeVehicle(criminalPassenger);
                }
            }

            if (criminal.Position.Distance(Game.PlayerPed.Position) < 400f && !isMissionStarted)
            {
                if (criminalPassenger != null)
                    API.RegisterHatedTargetsAroundPed(criminalPassenger.Handle, 400f);

                isMissionStarted = true;
            }

            if (criminal.Position.Distance(Game.PlayerPed.Position) > 600f && isMissionStarted)
            {
                missionState = MissionState.Escaped;
            }

            switch (missionState)
            {
                case MissionState.Started:
                    break;
                case MissionState.Escaped:
                    Fail("Vehicle got away....");
                    missionState = MissionState.End;
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

            ped.IsFleeing = true;

            TaskSequence taskSequence = new TaskSequence();

            if (ped.IsInVehicle)
                taskSequence.AddTask.LeaveVehicle(LeaveVehicleFlags.BailOut);

            taskSequence.AddTask.FleeFrom(Game.PlayerPed);
            ped.Task.PerformSequence(taskSequence);
            taskSequence.Close();
        }
    }
}
