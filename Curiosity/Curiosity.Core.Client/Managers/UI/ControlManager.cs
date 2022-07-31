using Curiosity.Core.Client.Extensions;

namespace Curiosity.Core.Client.Managers.UI
{
    public class ControlManager : Manager<ControlManager>
    {
        public bool CanScreenInteract => (!Game.IsPaused && !IsPauseMenuRestarting() && IsScreenFadedIn() && !IsPlayerSwitchInProgress() && Cache.Character.MarkedAsRegistered);

        public override void Begin()
        {

        }

        [TickHandler(SessionWait = true)]
        private async Task OnControlsTick()
        {
            if (Game.IsControlPressed(0, Control.MultiplayerInfo))
            {
                SetRadarBigmapEnabled(true, false);
                PluginManager.Instance.AttachTickHandler(OnShowVehicleStatisticsTickAsync);
                await BaseScript.Delay(10000);
                SetRadarBigmapEnabled(false, false);
                PluginManager.Instance.DetachTickHandler(OnShowVehicleStatisticsTickAsync);
            }
        }

        private async Task OnShowVehicleStatisticsTickAsync()
        {
            try
            {
                Vehicle[] closestVehicles = Game.PlayerPed.GetNearbyVehicles(20f);

                for (int i = 0; i < 10; i++)
                {
                    Scaleform scaleform = VehicleExtensions.CarStatScaleform;

                    switch (i)
                    {
                        case 0:
                            scaleform = VehicleExtensions.CarStatScaleform;
                            break;
                        case 1:
                            scaleform = VehicleExtensions.CarStatScaleform2;
                            break;
                        case 2:
                            scaleform = VehicleExtensions.CarStatScaleform3;
                            break;
                        case 3:
                            scaleform = VehicleExtensions.CarStatScaleform4;
                            break;
                        case 4:
                            scaleform = VehicleExtensions.CarStatScaleform5;
                            break;
                        case 5:
                            scaleform = VehicleExtensions.CarStatScaleform6;
                            break;
                        case 6:
                            scaleform = VehicleExtensions.CarStatScaleform7;
                            break;
                        case 7:
                            scaleform = VehicleExtensions.CarStatScaleform8;
                            break;
                        case 8:
                            scaleform = VehicleExtensions.CarStatScaleform9;
                            break;
                        case 9:
                            scaleform = VehicleExtensions.CarStatScaleform10;
                            break;
                    }

                    closestVehicles[i].DrawCarStats(scaleform);
                }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
