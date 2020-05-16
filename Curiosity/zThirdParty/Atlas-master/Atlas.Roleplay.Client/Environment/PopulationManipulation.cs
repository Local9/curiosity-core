using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Environment
{
    public class PopulationManipulation : Manager<PopulationManipulation>
    {
        public float Mulitplier { get; set; } = 1.0f;

        [TickHandler]
        private async Task OnTick()
        {
            API.SetVehicleDensityMultiplierThisFrame(Mulitplier);
            API.SetPedDensityMultiplierThisFrame(Mulitplier);
            API.SetRandomVehicleDensityMultiplierThisFrame(Mulitplier);
            API.SetParkedVehicleDensityMultiplierThisFrame(Mulitplier);
            API.SetScenarioPedDensityMultiplierThisFrame(Mulitplier, Mulitplier);

            await Task.FromResult(0);
        }
    }
}