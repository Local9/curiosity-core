using CitizenFX.Core.Native;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment
{
    class Vehicles
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicles:Remove", new Action<int>(OnVehicleRemove));
            client.RegisterTickHandler(OnTick);
        }

        static async void OnVehicleRemove(int vehicleHandle)
        {
            if (API.NetworkIsHost())
            {
                int handleId = API.NetworkGetEntityFromNetworkId(vehicleHandle);
                Debug.WriteLine($"OnVehicleRemove -> {vehicleHandle}");
                API.DeleteVehicle(ref handleId);
            }

            await Client.Delay(0);
        }

        static async Task OnTick()
        {
            int veh = API.GetVehiclePedIsTryingToEnter(Game.PlayerPed.Handle);
            if (veh != 0)
            {
                CitizenFX.Core.Vehicle vehicle = new CitizenFX.Core.Vehicle(veh);
                if (API.GetSeatPedIsTryingToEnter(Game.PlayerPed.Handle) == -1)
                {
                    int networkId = API.VehToNet(vehicle.Handle);
                    API.SetNetworkIdExistsOnAllMachines(networkId, true);
                    API.SetNetworkIdCanMigrate(networkId, true);
                    BaseScript.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", networkId);
                }
            }

            await Task.FromResult(0);
        }
    }
}
