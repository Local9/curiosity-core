using Curiosity.Framework.Client.GameInterface;
using Curiosity.Framework.Shared.Models.Hud;
using System.Drawing;

namespace Curiosity.Framework.Client.Extensions
{
    internal static class VehicleExtensions
    {
        public static void ShowLicensePlateAboveMinimap(this Vehicle vehicle)
        {
            string licensePlate = vehicle.Mods.LicensePlate;
            string displayPlate = string.IsNullOrWhiteSpace(licensePlate) ? "NO PLATE" : licensePlate;
            MinimapAnchor minimapAnchor = Hud.GetMinimapAnchor();
            Hud.DrawTextLegacy(displayPlate, 0.25f, new PointF(minimapAnchor.XPlusWidthDividedByTwo - 0.07f, minimapAnchor.YPlusHeightDividedByTwo - 0.11f), Color.FromArgb(255, 255, 0, 0));
        }
    }
}
