using CitizenFX.Core;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.UI
{
    public class ControlManager : Manager<ControlManager>
    {
        public bool CanScreenInteract => (!Game.IsPaused && !IsPauseMenuRestarting() && IsScreenFadedIn() && !IsPlayerSwitchInProgress() && Cache.Character.MarkedAsRegistered);

        public override void Begin()
        {
            
        }

        [TickHandler(SessionWait = true)]
        private async Task OnControlsTick()
        {
            if (Game.IsControlPressed(0, Control.MultiplayerInfo))
            {
                SetRadarBigmapEnabled(true, false);
                await BaseScript.Delay(10000);
                SetRadarBigmapEnabled(false, false);
            }
        }
    }
}
