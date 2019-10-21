using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Classes.Data;

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

        public static float Distance(this Position position, Position target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }
    }
}
