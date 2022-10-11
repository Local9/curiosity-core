namespace Curiosity.Framework.Client.Extensions
{
    internal static class VehicleExtensions
    {
        public static void ShowLicensePlateAboveMinimap(this Vehicle vehicle)
        {
            string licensePlate = vehicle.Mods.LicensePlate;
            string displayPlate = string.IsNullOrWhiteSpace(licensePlate) ? "NO PLATE" : licensePlate;
        }
    }
}
