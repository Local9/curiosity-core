using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class Building
    {
        public string Name { get; set; }
        public Quaternion Enterance { get; set; }
        public Quaternion Exit { get; set; }
        public Quaternion Lobby { get; set; }
        public BuildingCamera Camera { get; set; }
        public BuildingCamera EnteranceCamera1 { get; set; }
        public BuildingCamera EnteranceCamera2 { get; set; }
        public BuildingCamera EnteranceCamera3 { get; set; }
        public BuildingCamera EnteranceCamera4 { get; set; }
        public eGarageType GarageType { get; set; }
        public eBuildingType BuildingType { get; set; }
        public List<Apartment> Apartments { get; set; }
        public string[] HideObjects { get; set; }
        public Vector3 SaleSignPosition { get; set; }
        public eFrontDoor FrontDoor { get; set; }
        public Door Door1 { get; set; }
        public Door Door2 { get; set; }
        public Door Door3 { get; set; }
        public Blip BuildingBlip { get; set; }
        public Blip SaleSignBlip { get; set; }
    }
}
