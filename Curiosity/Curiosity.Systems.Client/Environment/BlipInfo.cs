using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Client.Extensions;

namespace Curiosity.Systems.Client.Environment
{
    public class BlipInfo
    {
        public string Name { get; set; } = "BlipInfo (Unknown name)";
        public BlipSprite Sprite { get; set; }
        public int Display { get; set; } = 4;
        public BlipColor Color { get; set; }
        public bool Route { get; set; }
        public int RouteColor { get; set; }
        public bool ShortRanged { get; set; } = true;
        public float Scale { get; set; } = 1f;
        public Position Position { get; set; }

        public void Commit()
        {
            Vector3 pos = new Vector3(Position.X, Position.Y, Position.Z);
            Blip blip = World.CreateBlip(pos);

            blip.Scale = Scale;
            API.SetBlipDisplay(blip.Handle, Display);
            blip.Sprite = Sprite;
            blip.Color = Color;
            blip.IsShortRange = ShortRanged;
            blip.ShowRoute = Route;
            API.SetBlipRouteColour(blip.Handle, RouteColor);
            blip.Name = Name;
        }
    }
}