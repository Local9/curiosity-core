﻿using CitizenFX.Core;
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

            Vector3 location = Players[0].Character.Position.AroundStreet(200f, 400f);

            Blip locationBlip = Functions.SetupLocationBlip(location);
            RegisterBlip(locationBlip);

            while (location.Distance(Game.PlayerPed.Position) > 50f)
            {
                await BaseScript.Delay(100);
            }

            if (locationBlip.Exists())
                locationBlip.Delete();

            stolenVehicle = await Vehicle.Spawn(vehicleHashes.Random(), location);

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

            Blip b = stolenVehicle.AttachSuspectBlip();

            if (b != null)
            {
                b.ShowRoute = true;
            }

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

                Mission.RegisterPed(criminalPassenger);

                criminalPassenger.PutInVehicle(stolenVehicle, VehicleSeat.Any);
            }

            stolenVehicle.IsSpikable = true;
            stolenVehicle.IsMission = true;
            stolenVehicle.IsTowable = true;

            Notify.Dispatch("~o~CODE 3 !! ARMED !!", $"~y~Stolen Vehicle~n~~b~Make~s~: {stolenVehicle.Name}~n~~b~Plate~s~: {stolenVehicle.Fx.Mods.LicensePlate}~n~~g~GPS Updated");

            isMissionReady = true;

            DiscordStatus("Chasing a Stolen Vehicle");

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            while(!isMissionReady)
            {
                await BaseScript.Delay(100);
            }

            if (criminal.Position.Distance(Game.PlayerPed.Position) < 200f && !isMissionStarted)
            {
                if (criminalPassenger != null)
                    API.RegisterHatedTargetsAroundPed(criminalPassenger.Handle, 300f);

                isMissionStarted = true;
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

            if (criminal.Position.Distance(Game.PlayerPed.Position) > 600f && isMissionStarted && NumberPedsArrested == 0)
            {
                missionState = MissionState.Escaped;
            }

            if (NumberPedsArrested >= 1)
                missionState = MissionState.End;

            if (NumberPedsArrested == 0)
            {
                if (criminal != null && criminalPassenger != null && criminal.IsDead && criminalPassenger.IsDead)
                    missionState = MissionState.SuspectDied;
            }

            switch (missionState)
            {
                case MissionState.Started:
                    break;
                case MissionState.Escaped:
                    Fail("Vehicle got away....");
                    break;
                case MissionState.SuspectDied:
                    Fail("Suspect(s) dead.");
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

            ped.AttachSuspectBlip();

            ped.IsFriendly = false;

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
