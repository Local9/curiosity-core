namespace Curiosity.Framework.Client.Managers.GameWorld
{
    public class WorldTimeManager : Manager<WorldTimeManager>
    {
        public override void Begin()
        {
            NetworkClearClockTimeOverride();
            int hour = 0;
            int minute = 0;
            int second = 0;
            NetworkGetGlobalMultiplayerClock(ref hour, ref minute, ref second);
            Logger.Debug($"NetworkGetGlobalMultiplayerClock - {hour:00}:{minute:00}:{second:00}");
        }
    }
}
