using CitizenFX.Core;
using System.Threading.Tasks;

namespace Curiosity.Vehicle.Client.net.Classes.Vehicle
{
    static class VehicleBlip
    {
        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(UpdateBlipIfInCar);
        }

        static async Task UpdateBlipIfInCar()
        {
            if (Client.CurrentVehicle == null)
            {
                await Task.FromResult(0);
                return;
            }

            if (Client.CurrentVehicle.Exists())
            {
                if (Client.CurrentVehicle.Driver.Handle == Game.PlayerPed.Handle)
                {
                    Client.CurrentVehicle.AttachedBlip.Alpha = 0;
                }
                else
                {
                    Client.CurrentVehicle.AttachedBlip.Alpha = 255;
                }
            }
            await Client.Delay(1000);
            await Task.FromResult(0);
        }
    }
}
