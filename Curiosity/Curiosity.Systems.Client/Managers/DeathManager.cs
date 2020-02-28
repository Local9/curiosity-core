using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Client.Extensions;
using Curiosity.Systems.Client.Interface;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Managers
{
    public class DeathManager : Manager<DeathManager>
    {
        public long Timestamp { get; set; }
        public bool WasDead { get; set; }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            var player = Cache.Player;
            var entity = Cache.Entity;

            if (entity.IsDead)
            {
                WasDead = true;
                
                if (Timestamp < 1)
                {
                    player.DisableHud();

                    Timestamp = Date.Timestamp;
                }

                var timeLeft = new TimeSpan(Date.TimestampToTicks(Timestamp + 1000 * 60 * 5 - Date.Timestamp));

                ScreenInterface.DrawText($"You are unconscious. ({timeLeft.Minutes}:{timeLeft.Seconds})~n~(Press E to re-emerge at the hospital)",
                    0.3f, new Vector2(0.5f, 0.75f), Color.FromArgb(175, 175, 175), true);

                if (Game.IsControlJustPressed(0, Control.DropWeapon))
                {
                    API.ClearPedTasksImmediately(entity.Id);
                }

                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    API.DoScreenFadeOut(0);

                    // TODO: Charge cash

                    player.Character.Revive(new Position(319.6946f, -573.5911f, 43.31747f, 231.9435f));

                    await BaseScript.Delay(3000);

                    API.DoScreenFadeIn(3000);
                }
                else if (timeLeft.Ticks < 1)
                {
                    API.DoScreenFadeOut(0);

                    player.Character.Revive(new Position(319.6946f, -573.5911f, 43.31747f, 231.9435f));

                    await BaseScript.Delay(3000);

                    API.DoScreenFadeIn(3000);
                }

                await Task.FromResult(0);
            }
            else
            {
                if (WasDead)
                {
                    Timestamp = 0;
                    
                    player.EnableHud();
                }

                await BaseScript.Delay(1000);
            }
        }
    }
}