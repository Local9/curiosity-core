using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Managers
{
    public class PopulationManipulation : Manager<PopulationManipulation>
    {
        public float Mulitplier { get; set; } = 1.0f;

        [TickHandler(SessionWait=true)]
        private async Task OnTick()
        {
            API.SetVehicleDensityMultiplierThisFrame(Mulitplier);
            API.SetPedDensityMultiplierThisFrame(Mulitplier);
            API.SetRandomVehicleDensityMultiplierThisFrame(Mulitplier);
            API.SetParkedVehicleDensityMultiplierThisFrame(Mulitplier);
            API.SetScenarioPedDensityMultiplierThisFrame(Mulitplier, Mulitplier);
        }
    }
}
