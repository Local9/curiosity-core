using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.World.Client.net.Classes.Environment
{
    class TimeSync
    {
        static Client client = Client.GetInstance();

        public static int currentHours = 9;
        public static int currentMinutes = 0;

        private static bool SmoothTimeTransitionsEnabled = true;
        private static bool DontDoTimeSyncRightNow = false;
        private static bool FreezeTime = false;

        private static int minuteTimer = GetGameTimer();
        private static int minuteClockSpeed = 2000;

        private static int currentServerHours = currentHours;
        private static int currentServerMinutes = currentMinutes;

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Player:World:SetTime", new Action<int, int, bool>(OnTimeSync));
            client.RegisterTickHandler(TimeSyncEvent);
        }

        static private async void OnTimeSync(int newHours, int newMinutes, bool freezeTime)
        {
            if (Player.PlayerInformation.IsDeveloper())
            {

            }

            currentServerHours = newHours;
            currentServerMinutes = newMinutes;

            bool IsTimeDifferenceTooSmall()
            {
                var totalDifference = 0;
                totalDifference += (newHours - currentHours) * 60;
                totalDifference += (newMinutes - currentMinutes);

                if (totalDifference < 15 && totalDifference > -120)
                    return true;

                return false;
            }

            FreezeTime = freezeTime;

            if (SmoothTimeTransitionsEnabled && !IsTimeDifferenceTooSmall())
            {
                if (!DontDoTimeSyncRightNow)
                {
                    bool frozen = freezeTime;
                    DontDoTimeSyncRightNow = true;
                    FreezeTime = freezeTime;

                    var oldSpeed = minuteClockSpeed;

                    while (currentHours != currentServerHours || currentMinutes != currentServerMinutes)
                    {
                        FreezeTime = false;
                        await Client.Delay(0);
                        minuteClockSpeed = 1;
                    }
                    FreezeTime = freezeTime;

                    minuteClockSpeed = oldSpeed;

                    DontDoTimeSyncRightNow = false;
                }
            }
            else
            {
                currentHours = currentServerHours;
                currentMinutes = currentServerMinutes;
            }
        }

        static private async Task TimeSyncEvent()
        {
            // If time is frozen...
            if (FreezeTime)
            {
                // Time is set every tick to make sure it never changes (even with some lag).
                await Client.Delay(0);
                NetworkOverrideClockTime(currentHours, currentMinutes, 0);
            }
            // Otherwise...
            else
            {
                if (minuteClockSpeed > 2000)
                {
                    await Client.Delay(2000);
                }
                else
                {
                    await Client.Delay(minuteClockSpeed);
                }
                // only add a minute if the timer has reached the configured duration (2000ms (2s) by default).
                if (GetGameTimer() - minuteTimer > minuteClockSpeed)
                {
                    currentMinutes++;
                    minuteTimer = GetGameTimer();
                }

                if (currentMinutes > 59)
                {
                    currentMinutes = 0;
                    currentHours++;
                }
                if (currentHours > 23)
                {
                    currentHours = 0;
                }
                NetworkOverrideClockTime(currentHours, currentMinutes, 0);
            }
        }
    }
}
