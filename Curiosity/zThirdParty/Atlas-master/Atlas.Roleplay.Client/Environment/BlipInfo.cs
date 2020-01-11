using Atlas.Roleplay.Library.Models;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment
{
    public class BlipInfo
    {
        public string Name { get; set; } = "BlipInfo (Unknown name)";
        public int Sprite { get; set; }
        public int Color { get; set; }
        public bool Route { get; set; }
        public int RouteColor { get; set; }
        public bool ShortRanged { get; set; } = true;
        public float Scale { get; set; } = 1f;
        public Position Position { get; set; }

        public void Commit()
        {
            var blip = API.AddBlipForCoord(Position.X, Position.Y, Position.Z);

            API.SetBlipScale(blip, Scale);
            API.SetBlipDisplay(blip, 4);
            API.SetBlipSprite(blip, Sprite);
            API.SetBlipColour(blip, Color);
            API.SetBlipAsShortRange(blip, ShortRanged);
            API.SetBlipRoute(blip, Route);
            API.SetBlipRouteColour(blip, RouteColor);
            API.BeginTextCommandSetBlipName("STRING");
            API.AddTextComponentString(Name);
            API.EndTextCommandSetBlipName(blip);
        }
    }
}