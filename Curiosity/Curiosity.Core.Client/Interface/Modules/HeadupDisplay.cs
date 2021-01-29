using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Managers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using static CitizenFX.Core.UI.Screen;

namespace Curiosity.Core.Client.Interface.Modules
{
    public class HeadupDisplay : Manager<HeadupDisplay>
    {
        public bool IsDisabled { get; set; } = false;

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            var curiosity = PluginManager.Instance;
            var player = curiosity.Local;
            var ped = API.PlayerPedId();

            if (player?.Entity != null && !IsDisabled && !API.IsPedSittingInAnyVehicle(ped))
            {
                var anchor = GetMinimapAnchor();

                // Base
                DrawObject(anchor.X + 0.0005f, (float)anchor.BottomY, anchor.Width + 0.0005f,
                    anchor.UnitY * 18f,
                    Color.FromArgb(126, 0, 0, 0));

                // Health
                DrawObject(anchor.X + 0.0005f, (float)anchor.BottomY - anchor.UnitY * 18f / 2 / 2, anchor.Width / 2,
                    anchor.UnitY * 18f / 2, Color.FromArgb(175, 57, 102, 67));

                DrawObject(anchor.X + 0.0005f, (float)anchor.BottomY - anchor.UnitY * 18f / 2 / 2,
                    anchor.Width / 2 / API.GetEntityMaxHealth(ped) * API.GetEntityHealth(ped),
                    anchor.UnitY * 18f / 2, Color.FromArgb(175, 114, 204, 114));

                // Armor
                DrawObject(anchor.X + 0.001f + anchor.Width / 2, (float)anchor.BottomY - anchor.UnitY * 18f / 2 / 2,
                    anchor.Width / 2, anchor.UnitY * 18f / 2, Color.FromArgb(175, 47, 92, 115));

                DrawObject(anchor.X + 0.001f + anchor.Width / 2, (float)anchor.BottomY - anchor.UnitY * 18f / 2 / 2,
                    anchor.Width / 2 / 100 * API.GetPedArmour(ped), anchor.UnitY * 18f / 2,
                    Color.FromArgb(175, 93, 182, 229));
            }
            Hud.IsRadarVisible = Game.PlayerPed.IsInVehicle();
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
                Width = (float)(scaleX * (resolutionX / (4 * aspectRatio))),
                Height = (float)(scaleY * (resolutionY / 5.674)),
                X = (float)(scaleX * (resolutionX * (0.05f * (Math.Abs(safezone - 1.0) * 10)))),
                BottomY = 1.0 - scaleY * (resolutionY * (0.05f * (Math.Abs(safezone - 1.0) * 10)))
            };

            anchor.RightX = anchor.X + anchor.Width;
            anchor.Y = (float)(anchor.BottomY - anchor.Height);
            anchor.UnitX = (float)scaleX;
            anchor.UnitY = (float)scaleY;

            return anchor;
        }

        private void DrawObject(float x, float y, float width, float height, Color color)
        {
            API.DrawRect(x + width / 2f, y - height / 2f, width, height, color.R, color.G, color.B, color.A);
        }
    }

    public class MinimapAnchor
    {
        public float X { get; set; }
        public double RightX { get; set; }
        public float Y { get; set; }
        public double BottomY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float UnitX { get; set; }
        public float UnitY { get; set; }
    }
}