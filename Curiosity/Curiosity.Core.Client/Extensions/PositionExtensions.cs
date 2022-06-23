using Curiosity.Systems.Library.Models;
using System.Linq;

namespace Curiosity.Core.Client.Extensions
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
            if (!useZ) return Vector3.Distance(position, target);

            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static Vector3 CenterOfVectors(this List<Vector3> vectors)
        {
            Vector3 sum = Vector3.Zero;
            if (vectors == null || vectors.Count == 0)
            {
                return sum;
            }

            foreach (Vector3 vec in vectors)
            {
                sum += vec;
            }
            return sum / vectors.Count;
        }

        public static float Angle(Vector3 from, Vector3 to)
        {
            Vector3 fromNorm = Vector3.Normalize(new Vector3(from.X, from.Y, from.Z));
            Vector3 toNorm = Vector3.Normalize(new Vector3(to.X, to.Y, to.Z));

            double dot = Vector3.Dot(fromNorm, toNorm);
            return (float)(System.Math.Acos((dot)) * (180.0 / System.Math.PI));
        }

        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 planeNormal)
        {
            Vector3 perpVector = Vector3.Cross(planeNormal, from);

            double angle = Angle(from, to);
            double dot = Vector3.Dot(perpVector, to);
            if (dot < 0)
            {
                angle *= -1;
            }

            return (float)angle;
        }
    }
}