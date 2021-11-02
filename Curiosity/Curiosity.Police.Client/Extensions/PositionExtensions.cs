using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Police.Client.Extensions
{
    public static class PositionExtensions
    {
        public static Vector3 AsVector(this Position position, bool setGround = false)
        {
            Vector3 positionV = new Vector3(position.X, position.Y, position.Z);
            float ground = position.Z;

            if (API.GetGroundZFor_3dCoord_2(position.X, position.Y, position.Z, ref ground, false) && setGround)
                positionV.Z = ground;

            return positionV;
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

        public static float Distance(this Position position, Position target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static float Distance(this Position position, Vector3 target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static Vector3 FindClosestPoint(this Vector3 startingPoint, IEnumerable<Vector3> points)
        {
            if (points.Count() == 0) return Vector3.Zero;

            return points.OrderBy(x => Vector3.Distance(startingPoint, x)).First();
        }

        public static float Distance(this Vector3 position, Vector3 target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }
    }
}