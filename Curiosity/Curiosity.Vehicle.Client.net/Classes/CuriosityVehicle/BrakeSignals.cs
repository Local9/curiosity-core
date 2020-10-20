using CitizenFX.Core;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle
{
    static class BrakeSignals
    {
        static PlayerList PlayerList = Plugin.players;
        static public void Init()
        {
            Plugin.GetInstance().RegisterTickHandler(OnTick);
        }

        static private Task OnTick()
        {
            PlayerList.Where(p => p.Character.IsInVehicle() && p.Character.CurrentVehicle.Driver == p.Character && p.Character.CurrentVehicle.IsEngineRunning && p.Character.CurrentVehicle.Speed < 4f && (p != Game.Player || (p == Game.Player && !ControlHelper.IsControlPressed(Control.VehicleAccelerate, false, ControlModifier.Any)))).ToList().ForEach(p => p.Character.CurrentVehicle.AreBrakeLightsOn = true);
            return Task.FromResult(0);
        }
    }
}
