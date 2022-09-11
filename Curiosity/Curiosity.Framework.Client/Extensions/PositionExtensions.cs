﻿using Curiosity.Framework.Shared.SerializedModels;

namespace Curiosity.Framework.Client.Extensions
{
    internal static class PositionExtensions
    {
        public static Vector3 AsVector(this Position position, bool setGround = false)
        {
            Vector3 positionV = new Vector3(position.X, position.Y, position.Z);
            float ground = position.Z;

            if (API.GetGroundZFor_3dCoord_2(position.X, position.Y, position.Z, ref ground, false) && setGround)
                positionV.Z = ground;

            return positionV;
        }

        public static bool IsPositionOccupied(this Vector3 vector3, float radius = 5f)
        {
            return API.IsPositionOccupied(vector3.X, vector3.Y, vector3.Z, radius, false, true, false, false, false, 0, false);
        }

        public static float ToHeading(this Vector2 vector2)
        {
            return GetHeadingFromVector_2d(vector2.X, vector2.Y);
        }

        public static float Distance(this Position position, Position target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static float Distance(this Position position, Vector3 target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static float Distance(this Vector3 position, Vector3 target, bool useZ = false)
        {
            if (!useZ) return Vector3.Distance(position, target);

            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public static Quaternion ToQuaternion(this Vector5 vector5)
        {
            return new Quaternion(vector5.Vector3.X, vector5.Vector3.Y, vector5.Vector3.Z, vector5.Vector2.ToHeading());
        }

        public static Vector3 GetGround(this Vector3 position)
        {
            var ray = World.Raycast(position, position - new Vector3(0, 0, 1000), IntersectOptions.Everything, Game.PlayerPed);
            if (ray.DitHit)
            {
                return ray.HitPosition;
            }
            return position;
        }

        public static Vector3 GetGroundWithWaterTest(this Vector3 position)
        {
            float groundZ = position.Z;
            if (GetGroundZFor_3dCoord_2(position.X, position.Y, position.Z, ref groundZ, false))
                position = new Vector3(position.X, position.Y, groundZ);

            float waterHeight = position.Z;

            if (TestVerticalProbeAgainstAllWater(position.X, position.Y, position.Z, 1, ref waterHeight))
            {
                position.Z = waterHeight;
            }
            return position;
        }
    }
}
