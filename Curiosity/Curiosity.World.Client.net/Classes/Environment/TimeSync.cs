using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net;

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
        private static int minuteClockSpeed = 10000;

        private static int currentServerHours = currentHours;
        private static int currentServerMinutes = currentMinutes;

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Player:World:SetTime", new Action<int, int, bool>(OnTimeSync));
            client.RegisterTickHandler(TimeSyncEvent);

            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
        }

        static private void OnPlayerSpawned(dynamic spawnData)
        {
            try
            {
                SetClockDate(DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
            }
            catch (InvalidTimeZoneException timeEx)
            {
                Log.Warn($"InvalidTimeZoneException: {timeEx.Message}");
            }
        }

        static private async void OnTimeSync(int newHours, int newMinutes, bool freezeTime)
        {
            if (Player.PlayerInformation.IsDeveloper())
            {

            }

            if (WeatherSystem.IsHalloween)
            {
                currentHours = 0;
                currentMinutes = 0;
                SetClockTime(0, 0, 0);
                NetworkOverrideClockTime(0, 0, 0);
                PauseClock(true);
            }
            else
            {

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
            else if (WeatherSystem.IsHalloween)
            {
                SetClockTime(0, 0, 0);
                NetworkOverrideClockTime(0, 0, 0);
                PauseClock(true);
            }
            // Otherwise...
            else
            {
                PauseClock(false);
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
                SetClockTime(currentHours, currentMinutes, 0);
                NetworkOverrideClockTime(currentHours, currentMinutes, 0);
            }
        }
    }
}
