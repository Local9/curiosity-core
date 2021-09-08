using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;
using System.Collections.Generic;

namespace Curiosity.TrafficStops.Missions
{
    [MissionInfo("Traffic Stop", "misTrafficStop", 0f, 0f, 0f, MissionType.TrafficStop, true, "None")]
    public class TrafficStop : Mission
    {
        Vehicle veh;
        Ped driver;

        bool vehicleFlee = false;
        bool vehicleFleeAfterStopping = false;
        bool pedsHaveWeapons = false;

        MissionState missionState;

        List<WeaponHash> weaponHashes = new List<WeaponHash>()
        {
            WeaponHash.Pistol,
            WeaponHash.SMG,
            WeaponHash.MicroSMG
        };

        public async override void Start()
        {
            driver = TrafficStopManager.Manager.tsDriver;
            driver.AddToMission();

            Logger.Debug($"Traffic Stop -> Driver {driver.Handle}");
            RegisteredPeds.Add(driver);

            await BaseScript.Delay(100);

            veh = TrafficStopManager.Manager.tsVehicle;
            veh.AddToMission();

            await BaseScript.Delay(100);

            veh.RecordLicensePlate();

            Logger.Debug($"Traffic Stop -> Vehicle {veh.Handle}");

            RegisteredVehicles.Add(veh);

            await BaseScript.Delay(100);

            vehicleFlee = Utility.RANDOM.Bool(0.33f);
            vehicleFleeAfterStopping = Utility.RANDOM.Bool(0.15f);
            pedsHaveWeapons = Utility.RANDOM.Bool(0.5f);

            TrafficStopManager.Manager.tsPassengers.ForEach(p =>
            {
                Logger.Debug($"Traffic Stop -> Passenger {p.Handle}");

                RegisteredPeds.Add(p);
                p.AddToMission();
            });

            if (vehicleFlee)
            {
                missionState = MissionState.VehicleIsFleeing;
                driver.RunSequence(Ped.Sequence.FLEE_IN_VEHICLE);
                driver.IsSuspect = true;
                SetPedCombatAttributes(driver.Handle, (int)CombatAttributes.BF_IgnoreTrafficWhenDriving, true);
            }
            else
            {
                // https://runtime.fivem.net/doc/natives/?_0x0FA6E4B75F302400
                // Pull over this will try to avoid
                API.TaskVehicleEscort(driver.Handle, veh.Handle, Game.PlayerPed.CurrentVehicle.Handle, 0, 8f, (int)Collections.CombinedVehicleDrivingFlags.Normal, 5f, 0, 0f);
                Notify.Info($"Vehicle is attempting to pull over.");
                missionState = MissionState.AwaitingVehicleToStop;
            }

            if (pedsHaveWeapons && vehicleFlee)
            {
                Mission.RegisteredPeds.ForEach(ped =>
                {
                    Game.PlayerPed.RelationshipGroup.SetRelationshipBetweenGroups(ped.Fx.RelationshipGroup, Relationship.Hate, true);
                    ped.Fx.RelationshipGroup = (uint)Collections.RelationshipHash.Gang1;
                    ped.Fx.Weapons.Give(weaponHashes[Utility.RANDOM.Next(weaponHashes.Count)], 999, false, true);
                    ped.Fx.DropsWeaponsOnDeath = false;
                    ped.Task.FightAgainstHatedTargets(300f);
                    SetPedCombatAttributes(ped.Handle, (int)CombatAttributes.BF_AlwaysFight, true);
                    SetPedCombatAttributes(ped.Handle, (int)CombatAttributes.BF_CanDoDrivebys, true);
                    SetPedCombatAttributes(ped.Handle, (int)CombatAttributes.BF_CanFightArmedPedsWhenNotArmed, true);
                    SetPedCombatAttributes(ped.Handle, (int)CombatAttributes.BF_CanTauntInVehicle, true);
                    ped.IsSuspect = true;
                });
            }

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            switch (missionState)
            {
                case MissionState.AwaitingVehicleToStop:
                    if (vehicleFleeAfterStopping && !Game.PlayerPed.IsInVehicle())
                    {
                        driver.RunSequence(Ped.Sequence.FLEE_IN_VEHICLE);
                        missionState = MissionState.VehicleIsFleeing;
                        SetPedCombatAttributes(driver.Handle, (int)CombatAttributes.BF_IgnoreTrafficWhenDriving, true);
                        driver.IsSuspect = true;
                    }
                    break;
                case MissionState.VehicleIsFleeing:
                    if (veh.Position.Distance(Game.PlayerPed.Position) > 300f)
                    {
                        Fail("They got away.", EndState.Fail);
                        missionState = MissionState.End;
                    }
                    break;
            }

            int totalNumberOfPlayers = Players.Count;
            int playersDead = 0;
            Players.ForEach(p =>
            {
                if (p.IsDead)
                    playersDead++;
            });

            if (playersDead == totalNumberOfPlayers)
                Fail("All Players have died.", EndState.FailPlayersDead);
        }

        enum MissionState
        {
            AwaitingVehicleToStop,
            VehicleIsFleeing,
            End
        }
    }
}
