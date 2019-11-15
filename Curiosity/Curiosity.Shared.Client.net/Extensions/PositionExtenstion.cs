using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Classes.Data;
using System.Threading.Tasks;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class PositionExtenstion
    {
        public static Vector3 AsVector(this Position position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        public static Vector3 AsVector(this RotatablePosition position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        public static Vector3 Rotations(this RotatablePosition position)
        {
            return new Vector3(position.Pitch, position.Roll, position.Yaw);
        }

        public static Position ToPosition(this Vector3 vector)
        {
            return new Position(vector.X, vector.Y, vector.Z);
        }

        public static float Distance(this Vector3 position, Vector3 target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static float Distance(this Position position, Position target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public async static Task<Vector3> Ground(this Vector3 position)
        {
            float posZ = 0.0f;

            for(int i = 800; i > 0 ; --i)
            {
                await BaseScript.Delay(0);
                API.RequestCollisionAtCoord(position.X, position.Y, i + 0.0f);
                if (API.GetGroundZFor_3dCoord(position.X, position.Y, i + 0.0f, ref posZ, false))
                {
                    position.Z = posZ;
                    break;
                }
            }

            return position;
        }
    }
}
