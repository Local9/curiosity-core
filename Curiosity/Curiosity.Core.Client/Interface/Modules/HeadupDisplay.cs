using Curiosity.Core.Client.Managers;
using System;
using System.Drawing;

namespace Curiosity.Core.Client.Interface.Modules
{
    public class HeadupDisplay : Manager<HeadupDisplay>
    {
        public bool IsDisabled { get; set; } = false;

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