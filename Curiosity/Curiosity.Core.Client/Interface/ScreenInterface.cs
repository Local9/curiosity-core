using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Drawing;

namespace Curiosity.Core.Client.Interface
{
    public class ScreenInterface
    {
        public static bool TimeoutStateValue = false;
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

        public static async void Draw3DTextTimeout(float x, float y, float z, string message, int timeout = 2500, float scaleMod = 20.0f, float distanceToHide = 20f)
        {
            TimeoutState(timeout);
            while (TimeoutStateValue)
            {
                await BaseScript.Delay(0);
                Draw3DText(x, y, z, message, scaleMod, distanceToHide);
            }
        }

        public static async void TimeoutState(int timeout)
        {
            TimeoutStateValue = true;
            await BaseScript.Delay(timeout);
            TimeoutStateValue = false;
        }

        public static void Draw3DText(Vector3 pos, string message, float scaleMod = 20.0f, float distanceToHide = 20.0f, float zOffset = 1.0f)
        {
            Draw3DText(pos.X, pos.Y, pos.Z, message, scaleMod, distanceToHide, zOffset);
        }

        public static void Draw3DText(float x, float y, float z, string message, float scaleMod = 20.0f, float distanceToHide = 20.0f, float zOffset = 1.0f)
        {
            float distance = (float)Math.Sqrt(GameplayCamera.Position.DistanceToSquared(new Vector3(x, y, z)));
            float scale = ((1 / distance) * 2) * GameplayCamera.FieldOfView / scaleMod;

            if (distance > distanceToHide)
            {
                return;
            }

            API.SetTextScale(0.0f * scale, 1.1f * scale);
            API.SetTextFont(0);
            API.SetTextProportional(true);
            API.SetTextColour(255, 255, 255, 255);
            API.SetTextDropshadow(0, 0, 0, 0, 255);
            API.SetTextEdge(2, 0, 0, 0, 150);
            API.SetTextDropShadow();
            API.SetTextOutline();

            API.SetDrawOrigin(x, y, z + zOffset, 0);

            API.SetTextEntry("STRING");
            API.SetTextCentre(true);
            API.AddTextComponentString(message);

            API.EndTextCommandDisplayText(0, 0);
            API.ClearDrawOrigin();
        }
    }
}