using static System.Math;

namespace Curiosity.Core.Client.Utils
{
    class CameraMath
    {
        public const float DegToRad = (float)Math.PI / 180.0f;

        /// <summary>
        /// Lerps two float values by a step
        /// </summary>
        /// <returns>lerped float value in between two supplied</returns>
        public static float Lerp(float current, float target, float by)
        {
            return current * (1 - by) + target * by;
        }

        /// <summary>
        /// Calculates angle between two vectors
        /// </summary>
        /// <returns>Angle between vectors in degrees</returns>
        public static float AngleBetween(Vector3 a, Vector3 b)
        {
            double sinA = a.X * b.Y - b.X * a.Y;
            double cosA = a.X * b.X + a.Y * b.Y;
            return (float)Math.Atan2(sinA, cosA) / DegToRad;
        }

        public static Vector3 RotateRadians(Vector3 v, float degree)
        {
            float ca = Cos(degree);
            float sa = Sin(degree);
            return new Vector3(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y, v.Z);
        }

        public static Vector3 RotateAroundAxis(Vector3 v, Vector3 axis, float angle)
        {
            return Vector3.TransformCoordinate(v, Matrix.RotationAxis(Vector3.Normalize(axis), angle));
        }

        public static float Fmod(float a, float b)
        {
            return (a - b * Floor(a / b));
        }

        public static Vector3 QuaternionToEuler(Quaternion q)
        {
            double r11 = (-2 * (q.X * q.Y - q.W * q.Z));
            double r12 = (q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z);
            double r21 = (2 * (q.Y * q.Z + q.W * q.X));
            double r31 = (-2 * (q.X * q.Z - q.W * q.Y));
            double r32 = (q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);

            float ax = (float)Math.Asin(r21);
            float ay = (float)Math.Atan2(r31, r32);
            float az = (float)Math.Atan2(r11, r12);

            return new Vector3(ax / DegToRad, ay / DegToRad, az / DegToRad);
        }
    }
}
