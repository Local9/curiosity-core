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
        public bool Stolen { get; set; }
        public string OwnerName { get; set; }
        public bool InsuranceValid { get; set; }

        public override string ToString()
        {
            return $"Vehicle;" +
                $"\n Display Name: {DisplayName}" +
                $"\n License Plate: {LicensePlate}" +
                $"\n Primary Color: {PrimaryColor}" +
                $"\n Secondary Color: {SecondaryColor}" +
                $"\n Stolen: {Stolen}" +
                $"\n Owner: {OwnerName}" +
                $"\n Insurance: {InsuranceValid}" +
                $"";
        }
    }
}