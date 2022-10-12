namespace Curiosity.Framework.Shared.Models.Hud
{
    public class MinimapAnchor
    {
        public float X { get; set; }
        public float RightX { get; set; }
        public float Y { get; set; }
        public float BottomY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float UnitX { get; set; }
        public float UnitY { get; set; }

        public float XPlusWidthDividedByTwo => X + Width / 2f;
        public float YMinusHeightDividedByTwo => Y - Height / 2f;
        public float YPlusHeightDividedByTwo => Y + Height / 2f;
    }
}
