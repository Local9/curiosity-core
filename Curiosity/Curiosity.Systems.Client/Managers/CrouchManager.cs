using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Managers
{
    public class Crouch : Manager<Crouch>
    {
        public bool IsCrouching { get; set; }

        public override async void Begin()
        {
            API.RequestAnimSet("move_ped_crouched");

            while (!API.HasAnimSetLoaded("move_ped_crouched"))
            {
                await BaseScript.Delay(10);
            }
        }


        [TickHandler(SessionWait = true)]
        private async Task OnCrouchTick()
        {
            if (Session.CreatingCharacter) return;
            if (Game.IsControlJustPressed(0, Control.Duck))
            {
                var ped = Cache.Entity.Id;

                API.DisableControlAction(0, 36, true);
                API.SetPedStealthMovement(ped, false, "");

                if (IsCrouching)
                {
                    API.SetPedMovementClipset(Cache.Entity.Id, "move_ped_crouched", 0.25f);
                }
                else
                {
                    API.ResetPedMovementClipset(Cache.Entity.Id, 0);
                }

                IsCrouching = !IsCrouching;
            }
        }
    }
}