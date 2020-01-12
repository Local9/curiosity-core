using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.System.Client.Extensions;
using Curiosity.System.Library.Models;

namespace Curiosity.System.Client.Environment
{
    public class WorldText
    {
        public static void Draw(string text, float scaleMultiplier, Position position)
        {
            var x = 0f;
            var y = 0f;
            var visible = API.GetScreenCoordFromWorldCoord(position.X, position.Y, position.Z, ref x, ref y);

            if (!visible) return;

            var camera = GameplayCamera.Position;
            var distance = camera.ToPosition().Distance(position, true);
            var scale = 0.5f / distance * 2f;
            var fov = 1f / GameplayCamera.FieldOfView * 100f;

            scale = scale * fov;

            API.SetTextScale(scale * scaleMultiplier, scale * scaleMultiplier);
            API.SetTextFont(0);
            API.SetTextProportional(true);
            API.SetTextColour(255, 255, 255, 255);
            API.SetTextDropshadow(0, 0, 0, 0, 255);
            API.SetTextEdge(2, 0, 0, 0, 150);
            API.SetTextDropShadow();
            API.SetTextOutline();
            API.SetTextEntry("STRING");
            API.SetTextCentre(true);
            API.AddTextComponentString(text);
            API.DrawText(x, y);
        }
    }
}