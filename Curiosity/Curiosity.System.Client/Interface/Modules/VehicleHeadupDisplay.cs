using System;
using System.Drawing;
using System.Threading.Tasks;
using Curiosity.System.Client.Managers;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.System.Client.Interface.Modules
{
    public class VehicleHeadupDisplay : Manager<VehicleHeadupDisplay>
    {
        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            var player = Cache.Player;

            if (player?.Entity != null && API.IsPedSittingInAnyVehicle(player.Entity.Id))
            {
                var entity = Cache.Entity;
                var anchor = GetMinimapAnchor();

                ScreenInterface.DrawText(
                    $"KM/H: {Math.Ceiling(API.GetEntitySpeed(API.GetVehiclePedIsIn(entity.Id, false)) * 3.6)}", 0.25f,
                    new Vector2(anchor.X + anchor.Width + 0.001f,
                        (float) (anchor.BottomY - anchor.Height / 2) - 0.025f),
                    Color.FromArgb(200, 200, 200, 200));
            }

            await Task.FromResult(0);
        }

        public MinimapAnchor GetMinimapAnchor()
        {
            var safezone = API.GetSafeZoneSize();
            var aspectRatio = API.GetAspectRatio(false);
            var resolutionX = 0;
            var resolutionY = 0;

            API.GetActiveScreenResolution(ref resolutionX, ref resolutionY);

            var scaleX = 1.0 / resolutionX;
            var scaleY = 1.0 / resolutionY;

            var anchor = new MinimapAnchor
            {
                Width = (float) (scaleX * (resolutionX / (4 * aspectRatio))),
                Height = (float) (scaleY * (resolutionY / 5.674)),
                X = (float) (scaleX * (resolutionX * (0.05f * (Math.Abs(safezone - 1.0) * 10)))),
                BottomY = 1.0 - scaleY * (resolutionY * (0.05f * (Math.Abs(safezone - 1.0) * 10)))
            };

            anchor.RightX = anchor.X + anchor.Width;
            anchor.Y = (float) (anchor.BottomY - anchor.Height);
            anchor.UnitX = (float) scaleX;
            anchor.UnitY = (float) scaleY;

            return anchor;
        }
    }
}