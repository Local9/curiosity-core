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

        static void OnVehicleRemove(int vehicleHandle)
        {
            API.SetEntityAsNoLongerNeeded(ref vehicleHandle);
            API.DeleteEntity(ref vehicleHandle);
        }
    }
}
