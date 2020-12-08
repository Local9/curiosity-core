namespace Curiosity.Systems.Shared.Entity
{
    public class MissionDataVehicle : MissionDataEntity
    {
        public bool IsTowable { get; set; }
        public bool RecordedLicensePlate { get; set; }
        public string LicensePlate { get; set; }
        public string DisplayName { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }

        public override string ToString()
        {
            return $"Vehicle: Towable: {IsTowable}, Blip: {AttachBlip}";
        }
    }
}