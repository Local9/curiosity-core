using Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    class SpawnMarkerHandler
    {
        public static void Init()
        {
            Marker marker = new Marker(VehicleSpawnTypes.Police, new CitizenFX.Core.Vector3(-1108.226f, -847.1646f, 19.31689f), CitizenFX.Core.MarkerType.CarSymbol, System.Drawing.Color.FromArgb(255, 135, 206, 250), 1.0f);
            // CAR MARKER
            MarkerHandler.All.Add(1, marker);

            // COLLISION MARKER
        }

        static void MarkerAction()
        {
            CitizenFX.Core.UI.Screen.ShowNotification("SPAWNER");
        }
    }
}
