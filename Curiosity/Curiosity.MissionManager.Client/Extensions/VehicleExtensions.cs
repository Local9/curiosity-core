using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Extensions
{
    public static class VehicleExtensions
    {

        public async static Task FadeOut(this Vehicle vehicle, bool slow = false)
        {
            await Fade(vehicle, false, slow);
        }

        public async static Task FadeIn(this Vehicle vehicle, bool slow = false)
        {
            await Fade(vehicle, true, slow);
        }

        public async static Task Fade(this Vehicle vehicle, bool fadeIn, bool slow = false)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(vehicle.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(vehicle.Handle, false, slow);
            }

            await BaseScript.Delay(3000);
        }
    }
}
