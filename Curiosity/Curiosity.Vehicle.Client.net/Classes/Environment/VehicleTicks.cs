using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    class VehicleTicks
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            client.RegisterTickHandler(OnExitingVehicleTask);
        }

        static async Task OnExitingVehicleTask()
        {
            await Task.FromResult(0);
            if (!Game.PlayerPed.IsInVehicle()) return;

            if (Game.IsControlPressed(2, Control.VehicleExit) && Game.PlayerPed.IsAlive && (Game.PlayerPed.CurrentVehicle.Model.IsCar || Game.PlayerPed.CurrentVehicle.Model.IsBike))
            {
                await BaseScript.Delay(150);
                if (Game.PlayerPed.IsInVehicle() && Game.IsControlPressed(2, Control.VehicleExit) && Game.PlayerPed.IsAlive)
                {
                    API.SetVehicleEngineOn(Game.PlayerPed.CurrentVehicle.Handle, true, true, false);
                    Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                }
                else
                {
                    Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.None);
                }
            }
        }
    }
}
