using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Library.Models;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Environment
{
    public class SafeTeleport
    {
        public static async Task TeleportFade(int entity, Position position, int interval = 10)
        {
            Screen.Fading.FadeOut(500);

            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(10);
            }

            API.FreezeEntityPosition(entity, true);

            await Teleport(entity, position, interval);

            Screen.Fading.FadeIn(500);

            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(10);
            }

            API.FreezeEntityPosition(entity, false);
        }

        public static async Task Teleport(int entity, Position position, int interval = 10)
        {
            API.RequestCollisionAtCoord(position.X, position.Y, position.Z);
            API.RequestAdditionalCollisionAtCoord(position.X, position.Y, position.Z);
            API.SetEntityCoordsNoOffset(entity, position.X, position.Y, position.Z, false, false, false);
            API.SetEntityHeading(entity, position.Heading);

            int failureCount = 0;

            while (!API.HasCollisionLoadedAroundEntity(entity))
            {
                if (failureCount > 10)
                    break;

                API.RequestCollisionAtCoord(position.X, position.Y, position.Z);
                API.RequestAdditionalCollisionAtCoord(position.X, position.Y, position.Z);

                failureCount++;
                await BaseScript.Delay(interval);
            }

            API.SetEntityCoordsNoOffset(entity, position.X, position.Y, position.Z, false, false, false);
            API.SetEntityHeading(entity, position.Heading);
        }
    }
}