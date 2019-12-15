using CitizenFX.Core;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Enums;

namespace Curiosity.Vehicle.Client.net.Classes.CuriosityVehicle
{
    static class BrakeSignals
    {
        static PlayerList PlayerList = Client.players;
        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static private Task OnTick()
        {
            PlayerList.Where(p => p.Character.IsInVehicle() && p.Character.CurrentVehicle.Driver == p.Character && p.Character.CurrentVehicle.IsEngineRunning && p.Character.CurrentVehicle.Speed < 4f && (p != Game.Player || (p == Game.Player && !ControlHelper.IsControlPressed(Control.VehicleAccelerate, false, ControlModifier.Any)))).ToList().ForEach(p => p.Character.CurrentVehicle.AreBrakeLightsOn = true);
            return Task.FromResult(0);
        }
    }
}
