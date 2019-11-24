using CitizenFX.Core;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class VehicleExtention
    {
        static public void SetVehicleIndicators(this Vehicle veh, bool state)
        {
            veh.IsRightIndicatorLightOn = state;
            veh.IsLeftIndicatorLightOn = state;
        }
    }
}
