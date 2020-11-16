using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;

namespace Curiosity.MissionManager.Client.Extensions
{
    public static class PositionExtensions
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

        public static float Distance(this Position position, Position target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static Vector3 Street(this Vector3 vector)
        {
            return World.GetNextPositionOnStreet(vector);
        }

        public static Vector3 Sidewalk(this Vector3 vector)
        {
            return World.GetNextPositionOnSidewalk(vector);
        }

        public static Vector3 AroundStreet(this Vector3 vector, float min, float max)
        {
            return World.GetNextPositionOnStreet(vector.Around(min, max));
        }

        public static Vector3 Around(this Vector3 vector, float min, float max)
        {
            Vector3 rotatedVector = Vector3.UnitY.Rotate((float)(Utility.RANDOM.NextDouble() * 360f));
            var scale = (float)(min + Utility.RANDOM.NextDouble() * (max - min));
            Vector3 result = vector + Vector3.Multiply(rotatedVector, scale);
            Debug.WriteLine(result.ToString());

            return result;
        }

        public static Vector3 Rotate(this Vector3 vector, float degrees)
        {
            var x = (float)(vector.X * Math.Cos(degrees) - vector.Y * Math.Sin(degrees));
            var y = (float)(vector.Y * Math.Cos(degrees) + vector.X * Math.Sin(degrees));

            return new Vector3(x, y, vector.Z);
        }
    }
}