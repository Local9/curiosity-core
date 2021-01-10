using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Environment
{
    public class SafeTeleport
    {
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