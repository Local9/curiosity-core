using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Inventory;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Environment
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

                var timeLeft = new TimeSpan(Date.TimestampToTicks(Timestamp + 1000 * 60 * 20 - Date.Timestamp));

                ScreenInterface.DrawText(
                    timeLeft.Ticks > 1
                        ? $"Du är medvetslös. ({timeLeft.Minutes}:{timeLeft.Seconds})"
                        : "Du är medvetslös. (Tryck E för att återuppstå vid sjukhuset)",
                    0.3f, new Vector2(0.5f, 0.75f), Color.FromArgb(175, 175, 175), true);

                if (Game.IsControlJustPressed(0, Control.DropWeapon))
                {
                    API.ClearPedTasksImmediately(entity.Id);
                }

                if (timeLeft.Ticks < 1 && Game.IsControlJustPressed(0, Control.Context) &&
                    InterfaceManager.GetModule().MenuContext == null)
                {
                    API.DoScreenFadeOut(0);

                    player.Character.Revive(new Position(319.6946f, -573.5911f, 43.31747f, 231.9435f));

                    ItemHelper.RemoveAll(InventoryManager.GetModule().GetContainer("equipment_inventory"), self => true);
                    ItemHelper.RemoveAll(InventoryManager.GetModule().GetContainer("pockets_inventory"), self => true);

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