using ScaleformUI;

namespace Curiosity.Framework.Client.Managers.GameWorld
{
    public class WorldTimeManager : Manager<WorldTimeManager>
    {
        int hour = 0;
        int minute = 0;
        int second = 0;

        public override void Begin()
        {
            NetworkClearClockTimeOverride();
            NetworkGetGlobalMultiplayerClock(ref hour, ref minute, ref second);
            NetworkOverrideClockTime(hour, minute, second);
            Logger.Debug($"BEGIN: NetworkGetGlobalMultiplayerClock - {hour:00}:{minute:00}:{second:00}");
        }

        // [TickHandler]
        private async Task OnShowOtherNetworkClockAsync()
        {
            NetworkGetGlobalMultiplayerClock(ref hour, ref minute, ref second);
            Notifications.DrawText(0.35f, 0.7f, $"{hour:00}:{minute:00}:{second:00}");
        }
    }
}
