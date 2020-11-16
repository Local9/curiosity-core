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

        public static async Task PlayScenario(this Ped ped, string scenario, int waitTime, bool enterAnim = false)
        {
            if (!ped.IsInVehicle())
            {
                API.TaskStartScenarioInPlace(ped.Handle, scenario, 0, enterAnim);

                int startTime = API.GetGameTimer();

                while ((API.GetGameTimer() - startTime) < waitTime)
                {
                    await BaseScript.Delay(100);
                }

                ped.Task.ClearAll();
            }
        }
    }
}
