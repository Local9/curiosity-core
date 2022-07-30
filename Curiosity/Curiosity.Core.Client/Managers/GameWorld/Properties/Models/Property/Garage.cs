namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models.Property
{
    internal class Garage
    {
        public Apartment Apartment { get; set; }
        public Camera Camera { get; set; }
        public Vector3 PositionInterior { get; set; }
        public Vector3 PositionElevator { get; set; }
        public Quaternion PositionElevatorInside { get; set; }
        public Quaternion PositionSpawnIn { get; set; }
        public Vector3 PositionGarageDoor { get; set; }
        public Vector3 PositionMenu { get; set; }
        public BuildingCamera PositionMenuCamera { get; set; }
    }
}
