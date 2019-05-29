using CitizenFX.Core.Native;
using System;

namespace Curiosity.Client.net.Classes.Environment
{
    class Vehicles
    {
        public static void Init()
        {
            Client.GetInstance().RegisterEventHandler("curiosity:Server:Vehicles:Remove", new Action<int>(OnVehicleRemove));
        }

        static async void OnVehicleRemove(int vehicleHandle)
        {
            int networkId = API.NetworkGetNetworkIdFromEntity(vehicleHandle);

            if (API.NetworkIsHost())
            {
                API.DeleteVehicle(ref networkId);
            }

            await Client.Delay(0);
        }
    }
}
