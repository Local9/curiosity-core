using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Extensions;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class HideReticle
    {
        static public void Init()
        {
            // TODO: Uncomment (just annoying LOL)
            Client.GetInstance().RegisterTickHandler(OnTickHideReticle);
        }

        static public async Task OnTickHideReticle()
        {
            bool isAimCamActive = Function.Call<bool>(Hash.IS_AIM_CAM_ACTIVE);
            bool isFirstPersonAimCamActive = Function.Call<bool>(Hash.IS_FIRST_PERSON_AIM_CAM_ACTIVE);

            if (Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_CROSSHAIR)) return;

            if (!isAimCamActive || !isFirstPersonAimCamActive)
            {
                CitizenFX.Core.UI.Screen.Hud.HideComponentThisFrame(CitizenFX.Core.UI.HudComponent.Reticle);
            }
        }
    }
}
