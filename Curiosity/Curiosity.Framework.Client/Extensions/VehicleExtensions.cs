using Curiosity.Framework.Client.GameInterface;
using Curiosity.Framework.Shared.Models.Hud;
using ScaleformUI;

namespace Curiosity.Framework.Client.Extensions
{
    internal static class VehicleExtensions
    {
        public static void ShowLicensePlateAboveMinimap(this Vehicle vehicle)
        {
            string licensePlate = vehicle.Mods.LicensePlate;
            string displayPlate = string.IsNullOrWhiteSpace(licensePlate) ? "NO PLATE" : licensePlate;
            MinimapAnchor minimapAnchor = Hud.GetMinimapAnchor();
            new UIResText($"~b~{displayPlate}", new System.Drawing.PointF(minimapAnchor.X - .003f, minimapAnchor.Y), 0.5f).Draw();
        }
    }
}
