using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Classes;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.MissionManager.Shared.Utils;
using Curiosity.Shared.Client.net;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.Development.Missions
{
    [MissionInfo("Development Vehicle", "devVehDmg", 0f, 0f, 0f, MissionType.StolenVehicle, true, "None")]
    public class SpawnVehicleDamaged : Mission
    {
        Vehicle _vehicle;

        public async override void Start()
        {
            _vehicle = await Vehicle.Spawn(VehicleHash.Adder, Players[0].Character.Position.Around(3f, 4f));

            RegisteredVehicles.Add(_vehicle);

            // windscreen
            _vehicle.DamageTop(numberOfHits: 2);
            _vehicle.DamageFront();

            _vehicle.BurstWheel(Wheels.FRONT_LEFT, true);
            _vehicle.BurstWheel(Wheels.FRONT_RIGHT);
            _vehicle.BurstWheel(Wheels.REAR_LEFT);
            _vehicle.BurstWheel(Wheels.REAR_RIGHT);

            Vector3 offset = new Vector3(-1.2f, -2.2f, 0f);
            Vector3 vehOffset = _vehicle.Fx.GetOffsetPosition(offset);

            Model fire = "vfx_it3_38";

            Prop prop = await World.CreateProp(fire, offset, false, true);

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
