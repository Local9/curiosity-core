namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class BuildingCamera
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public float FieldOfView;

        public BuildingCamera(Vector3 position, Vector3 rotation, float fieldOfView)
        {
            Position = position;
            Rotation = rotation;
            FieldOfView = fieldOfView;
        }

        public BuildingCamera Zero()
        {
            return new BuildingCamera(Vector3.Zero, Vector3.Zero, 0f);
        }
    }
}
