using CitizenFX.Core;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Server.Extensions
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
    }
}
