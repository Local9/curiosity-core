using CitizenFX.Core;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.MissionManager.Client.Utils
{
    class NativeWrapper
    {
        static public void Draw3DText(float x, float y, float z, string message, float scaleMod = 20.0f, float distanceToHide = 20.0f)
        {
            float distance = (float)Math.Sqrt(GameplayCamera.Position.DistanceToSquared(new Vector3(x, y, z)));
            float scale = ((1 / distance) * 2) * GameplayCamera.FieldOfView / scaleMod;

            if (distance > distanceToHide)
            {
                return;
            }

            SetTextScale(0.0f * scale, 1.1f * scale);
            SetTextFont(0);
            SetTextProportional(true);
            SetTextColour(255, 255, 255, 255);
            SetTextDropshadow(0, 0, 0, 0, 255);
            SetTextEdge(2, 0, 0, 0, 150);
            SetTextDropShadow();
            SetTextOutline();

            SetDrawOrigin(x, y, z + 1, 0);

            SetTextEntry("STRING");
            SetTextCentre(true);
            AddTextComponentString(message);

            EndTextCommandDisplayText(0, 0);
            ClearDrawOrigin();
        }
    }
}
