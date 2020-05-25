using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Extensions;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.net.Environment.Vehicle
{
    class LoadoutPosition
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            client.DeregisterTickHandler(OnVehicleBootTask);
            client.RegisterTickHandler(OnVehicleBootTask);
        }
        static public void Dispose()
        {
            client.DeregisterTickHandler(OnVehicleBootTask);
        }

        static async Task OnVehicleBootTask()
        {
            await Task.FromResult(0);

            if (Client.CurrentVehicle == null) return;
            if (Client.CurrentVehicle.IsDead) return;

            EntityBone entityBone = Client.CurrentVehicle.Bones["boot"];
            if (
                (entityBone.Position.Distance(Game.PlayerPed.Position) < 1.5
                 || (Client.CurrentVehicle.Model.IsBike && Client.CurrentVehicle.Position.Distance(Game.PlayerPed.Position) < 1.5))
                 && !Game.PlayerPed.IsInVehicle()
                 && Client.CurrentVehicle.ClassType == VehicleClass.Emergency
                 )
            {
                Screen.DisplayHelpTextThisFrame("Press ~INPUT_PICKUP~ to ~b~rearm.");

                if (Game.IsControlJustPressed(0, Control.Pickup))
                {
                    Game.PlayerPed.Task.TurnTo(Client.CurrentVehicle);
                    Client.CurrentVehicle.Doors[VehicleDoorIndex.Trunk].Open();
                    await Client.Delay(500);
                    AnimationSearch();
                    await Client.Delay(5100);
                    Classes.Menus.MenuLoadout.OnWeaponResupply();
                    Client.CurrentVehicle.Doors[VehicleDoorIndex.Trunk].Close();
                    await Client.Delay(10000);
                }
            }
        }

        static public async void AnimationSearch()
        {
            string scenario = "PROP_HUMAN_BUM_BIN";
            if (!Game.PlayerPed.IsInVehicle())
            {
                TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenario, 0, true);
                await Client.Delay(5000);
                Game.PlayerPed.Task.ClearAll();
            }
        }
    }
}
