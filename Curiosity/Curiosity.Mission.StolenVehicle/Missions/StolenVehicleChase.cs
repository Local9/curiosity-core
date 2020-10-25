using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.MissionManager.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.StolenVehicle.Missions
{
    [MissionInfo("Stolen Vehicle", "misSvChase", 485.979f, -1311.222f, 29.249f, MissionType.StolenVehicle, true, "None")]
    public class StolenVehicleChase : Mission
    {
        private MissionState missionState;
        private Ped criminal;
        private Vehicle stolenVehicle;

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
            Mission.RegisterVehicle(stolenVehicle);

            criminal = await Ped.Spawn(pedHashes.Random(), stolenVehicle.Position, true);

            criminal.IsPersistent = true;
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

            stolenVehicle.IsSpikable = true;
            stolenVehicle.IsMission = true;
            stolenVehicle.IsTowable = true;

            UiTools.Dispatch("~r~CODE 3~s~: Stolen Vehicle", $"~b~Make~s~: {stolenVehicle.Name}~n~~b~Plate~s~: {stolenVehicle.Fx.Mods.LicensePlate}");

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);

        }

        async Task OnMissionTick()
        {
            if (criminal.Position.Distance(Game.PlayerPed.Position) > 600f)
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
    }

    enum MissionState
    {
        Started,
        ChaseActive,
        Escaped,
        End
    }
}
