using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using Curiosity.Template.Client.Extensions;
using Curiosity.Template.Client.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Template.Client.Managers
{
    public class DeathManager : Manager<DeathManager>
    {
        public long Timestamp { get; set; }
        public bool WasDead { get; set; }

        static List<Vector3> Hospitals = new List<Vector3>()
        {
            new Vector3(297.8683f, -584.3318f, 43.25863f),
            new Vector3(356.434f, -598.5284f, 28.78098f),
            new Vector3(307.5486f, -1434.502f, 29.86082f),
            new Vector3(342.1533f, -1397.199f, 32.50924f),
            new Vector3(-496.291f, -336.9345f, 34.50164f),
            new Vector3(-449.0542f, -339.1804f, 34.50176f),
            new Vector3(1827.909f, 3691.912f, 34.22427f),
            new Vector3(-243.5568f, 6326.441f, 32.42619f),
            new Vector3(4773.674f, -1935.709f, 17.06867f)
        };

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

                int r = CuriosityPlugin.Rand.Next(Hospitals.Count);

                Vector3 playerPos = Game.PlayerPed.Position;

                Vector3 pos = new Vector3();

                foreach (Vector3 hosPos in Hospitals)
                {
                    float distance = API.GetDistanceBetweenCoords(playerPos.X, playerPos.Y, playerPos.Z, hosPos.X, hosPos.Y, hosPos.Z, false);

                    if (distance < 3000f)
                    {
                        pos = hosPos;
                        break;
                    }
                }

                if (pos.IsZero)
                {
                    pos = Hospitals[r];
                }

                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    API.DoScreenFadeOut(0);

                    // TODO: Charge cash

                    player.Character.Revive(new Position(pos.X, pos.Y, pos.Z, Game.PlayerPed.Heading));

                    await BaseScript.Delay(3000);

                    API.DoScreenFadeIn(3000);
                }
                else if (timeLeft.Ticks < 1)
                {
                    API.DoScreenFadeOut(0);

                    player.Character.Revive(new Position(pos.X, pos.Y, pos.Z, Game.PlayerPed.Heading));

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