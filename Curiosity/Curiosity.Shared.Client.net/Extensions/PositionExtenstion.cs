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

        public static float DistanceTo(this Vector3 position, Vector3 target) => (position - target).Length();

        public static float Distance(this Vector3 position, Vector3 target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static float Distance(this Position position, Position target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public async static Task<Vector3> Ground(this Vector3 position, bool vehicleNodeCheck = true)
        {
            float posZ = 0.0f;

            for(int i = 0; i < 800 ; i++)
            {
                await BaseScript.Delay(0);

                Vector3 vehicleNode = position;
                int nodeId = 0;

                if (API.GetGroundZFor_3dCoord(position.X, position.Y, position.Z, ref posZ, false))
                {
                    position.Z = posZ;
                }
                else
                {
                    if (vehicleNodeCheck)
                        API.GetRandomVehicleNode(position.X, position.Y, i + 0.0f, 5f, false, false, false, ref vehicleNode, ref nodeId);

                    API.RequestCollisionAtCoord(vehicleNode.X, vehicleNode.Y, i + 0.0f);
                    if (API.GetGroundZFor_3dCoord(vehicleNode.X, vehicleNode.Y, i + 0.0f, ref posZ, false))
                    {
                        position.Z = posZ;
                        break;
                    }
                }
            }

            return position;
        }
    }
}
