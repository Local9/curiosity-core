﻿using Curiosity.Framework.Shared.Models;

namespace Curiosity.Framework.Shared.Extensions
{
    internal static class PositionExtensions
    {

        public static Vector3 AsVector(this RotatablePosition position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        public static Vector3 AsVector(this Quaternion position)
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

        public static Position ToPosition(this Quaternion quarternion)
        {
            return new Position(quarternion.X, quarternion.Y, quarternion.Z);
        }

        public static Vector3 FindClosestPoint(this Vector3 startingPoint, IEnumerable<Vector3> points)
        {
            if (points.Count() == 0) return Vector3.Zero;

            return points.OrderBy(x => Vector3.Distance(startingPoint, x)).First();
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
