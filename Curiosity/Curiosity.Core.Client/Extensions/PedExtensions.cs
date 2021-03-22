using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Extensions
{
    public static class PedExtensions
    {
        public static async Task<string> GetHeadshot(this Ped ped)
        {
            int headshot = API.RegisterPedheadshot(ped.Handle);

            while (!API.IsPedheadshotReady(headshot))
            {
                await BaseScript.Delay(0);
            }

            return API.GetPedheadshotTxdString(headshot);
        }

        public async static Task FadeOut(this Ped ped, bool slow = true)
        {
            await Fade(ped, false, slow);
        }

        public async static Task FadeIn(this Ped ped, bool slow = true)
        {
            await Fade(ped, true, slow);
        }

        public async static Task Fade(this Ped ped, bool fadeIn, bool slow = true)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(ped.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(ped.Handle, false, slow);
            }

            while (API.NetworkIsEntityFading(ped.Handle))
            {
                await BaseScript.Delay(10);
            }

            Cache.UpdatePedId();
        }
    }
}
