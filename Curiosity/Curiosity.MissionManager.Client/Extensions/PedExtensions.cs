using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Extensions
{
    public static class PedExtensions
    {

        public async static Task FadeOut(this Ped ped, bool slow = false)
        {
            await Fade(ped, false, slow);
        }

        public async static Task FadeIn(this Ped ped, bool slow = false)
        {
            await Fade(ped, true, slow);
        }

        public async static Task Fade(this Ped ped, bool fadeIn, bool slow = false)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(ped.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(ped.Handle, false, slow);
            }

            await BaseScript.Delay(3000);
        }

        public static bool IsPlayingAnim(this Ped ped, string animSet, string animName)
        {
            return API.IsEntityPlayingAnim(ped.Handle, animSet, animName, 3);
        }
    }
}
