using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.Development.Missions
{
    [MissionInfo("Development Vehicle", "devVeh", 0f, 0f, 0f, MissionType.StolenVehicle, true, "None")]
    public class SpawnVehicle : Mission
    {
        Vehicle _vehicle;

        Vector3 fireOffset = new Vector3(0.0f, -1.4f, 0f);

        public async override void Start()
        {
            _vehicle = await Vehicle.Spawn(VehicleHash.Adder, Players[0].Character.Position.Around(3f, 4f));

            RegisteredVehicles.Add(_vehicle);

            _vehicle.Fx.IsEngineRunning = false;
            _vehicle.IsSearchable = true;
            _vehicle.Fx.IsDriveable = false;
            _vehicle.Fx.FuelLevel = 0;
            _vehicle.Fx.LockStatus = VehicleLockStatus.Locked;

            _vehicle.IsMission = true;

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {

        }
    }
}
