using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class Garage
    {
        public Vector3 Position { get; set; }
        public eGarageType GarageType { get; set; }
        public Quaternion CarEnterance { get; set; }
        public Quaternion CarExit { get; set; }
        public Quaternion FootEnterance { get; set; }
        public Quaternion FootExit { get; set; }
        public eFrontDoor Door { get; set; }
        public Quaternion Waypoint { get; set; }
    }
}
