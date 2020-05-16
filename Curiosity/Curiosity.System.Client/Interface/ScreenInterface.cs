using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Drawing;

namespace Curiosity.System.Client.Interface
{
    public class ScreenInterface
    {
        public static void DrawText(string text, float scale, Vector2 position, Color color, bool centered = false)
        {
            API.SetTextFont(0);
            API.SetTextProportional(true);
            API.SetTextScale(scale, scale);
            API.SetTextColour(color.R, color.G, color.B, color.A);
            API.SetTextDropshadow(0, 0, 0, 0, 255);
            API.SetTextEdge(1, 0, 0, 0, 150);
            API.SetTextDropShadow();
            API.SetTextOutline();
            API.SetTextCentre(centered);
            API.BeginTextCommandDisplayText("STRING");
            API.AddTextComponentSubstringPlayerName(text);
            API.EndTextCommandDisplayText(position.X, position.Y);
        }
    }
}