using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Environment
{
    public class Handsup : Manager<Handsup>
    {
        public bool HandsUp { get; set; }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            if (Session.CreatingCharacter) return;
            if (!API.IsPedInAnyVehicle(Cache.Entity.Id, false))
            {
                if (Game.IsControlJustPressed(0, Control.VehicleDuck) && !HandsUp)
                {
                    HandsUp = true;

                    Game.PlayerPed.Task.PlayAnimation("random@mugging3", "handsup_standing_base", 8f, -1,
                        AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly |
                        AnimationFlags.AllowRotation);
                }
                else if (Game.IsControlJustReleased(0, Control.VehicleDuck) && HandsUp)
                {
                    API.ClearPedTasksImmediately(Cache.Entity.Id);
                    API.ClearPedSecondaryTask(Cache.Entity.Id);

                    HandsUp = false;
                }
            }

            await Task.FromResult(0);
        }
    }
}