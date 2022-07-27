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
    }
}
