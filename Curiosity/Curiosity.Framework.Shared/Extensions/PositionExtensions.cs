using Curiosity.Framework.Shared.SerializedModels;

namespace Curiosity.Framework.Shared.Extensions
{
    internal static class PositionExtensions
    {
        public static Quaternion LookAt(Vector3 Start, Vector3 End)
        {
            Vector3 ForwardVector = Vector3.Normalize(End - Start);

            float dot = Vector3.Dot(Vector3.ForwardLH, ForwardVector);

            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                return new Quaternion(Vector3.Up.Z, Vector3.Up.Z, Vector3.Up.Z, (float)Math.PI);
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                return Quaternion.Identity;
            }

            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(Vector3.ForwardLH, ForwardVector);
            rotAxis = Vector3.Normalize(rotAxis);
            return CreateFromAxisAngle(rotAxis, rotAngle);

        }

        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float halfAngle = angle * .5f;
            float s = (float)Math.Sin(halfAngle);
            Quaternion q = new Quaternion();
            q.X = axis.X * s;
            q.X = axis.Y * s;
            q.Z = axis.Z * s;
            q.W = (float)Math.Cos(halfAngle);
            return q;
        }
        
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
