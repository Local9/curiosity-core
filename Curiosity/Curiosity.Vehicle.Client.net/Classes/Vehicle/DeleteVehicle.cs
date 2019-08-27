using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicle.Client.net.Classes.Vehicle
{
    class DeleteVehicle
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Player:Vehicle:Delete", new Action<string>(OnDelete));
        }

        static void OnDelete(string vehicleNetworkId)
        {
            try
            {
                if (string.IsNullOrEmpty(vehicleNetworkId)) return;

                int vehicleId = NetworkGetEntityFromNetworkId(int.Parse(vehicleNetworkId));

                CitizenFX.Core.Vehicle vehicle = new CitizenFX.Core.Vehicle(vehicleId);

                if (vehicle != null)
                {
                    if (vehicle.Exists())
                    {
                        if (vehicle.AttachedBlip.Exists())
                        {
                            vehicle.AttachedBlip.Delete();
                        }

                        NetworkFadeOutEntity(vehicleId, true, false);
                        vehicle.IsEngineRunning = false;
                        vehicle.MarkAsNoLongerNeeded();
                        vehicle.Delete();

                        if (vehicle.Exists())
                        {
                            DeleteEntity(ref vehicleId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDelete -> {ex.Message}");
            }
        }
    }
}
