using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.Systems.Library.Enums;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.Development.Missions
{
    [MissionInfo("Development Vehicle Damaged", "devVehDmg", 0f, 0f, 0f, MissionType.Developer, true, "None")]
    public class SpawnVehicleDamaged : Mission
    {
        Vehicle _vehicle;

        Vector3 fireOffset = new Vector3(0.0f, -1.4f, 0f);

        public async override void Start()
        {
            _vehicle = await Vehicle.Spawn(VehicleHash.Adder, Players[0].Character.Position.Around(3f, 4f));

            RegisteredVehicles.Add(_vehicle);

            _vehicle.Fx.IsEngineRunning = false;
            _vehicle.Fx.IsDriveable = false;
            _vehicle.IsSearchable = false;
            _vehicle.Fx.FuelLevel = 0;
            _vehicle.Fx.LockStatus = VehicleLockStatus.Locked;

            // windscreen
            _vehicle.DamageTop(numberOfHits: 2);
            _vehicle.DamageFront();

            _vehicle.BurstWheel(Vehicle.Wheels.FRONT_LEFT, true);
            _vehicle.BurstWheel(Vehicle.Wheels.FRONT_RIGHT);
            _vehicle.BurstWheel(Vehicle.Wheels.REAR_LEFT);
            _vehicle.BurstWheel(Vehicle.Wheels.REAR_RIGHT);

            await BaseScript.Delay(10);

            // X : - LEFT/RIGHT +
            // Y : - BACK/FORWARD +
            // Z : - DOWN/UP +

            Vector3 offset = fireOffset;
            Vector3 offset2 = offset;
            offset2.Y = 1.3f;
            Vector3 rotation = new Vector3(0f);
            Vector3 vehOffset = _vehicle.Fx.GetOffsetPosition(offset);

            _vehicle.ParticleEffect("core", "fire_wrecked_car", offset, 1.5f);
            _vehicle.ParticleEffect("core", "fire_wrecked_car", offset2, 1.5f);

            _vehicle.Fx.Doors[VehicleDoorIndex.FrontLeftDoor].Break();
            _vehicle.Fx.Doors[VehicleDoorIndex.FrontLeftDoor].Open();

            _vehicle.IsMission = true;

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            Stop(EndState.Error);
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            if (API.IsShockingEventInSphere(88, fireOffset.X, fireOffset.Y, fireOffset.Z, 10f))
                Screen.ShowSubtitle("Hit");

            if (API.IsShockingEventInSphere(89, fireOffset.X, fireOffset.Y, fireOffset.Z, 10f))
                Screen.ShowSubtitle("Hit 2");
        }
    }
}
