using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.World.Server.net.Classes.Environment
{
    class WorldTimeCycle
    {
        static Server server = Server.GetInstance();

        static private int currentHours = 9;
        static private int currentMinutes = 0;
        static private readonly int minuteClockSpeed = 10000; // TODO: Make a server variable
        static private long minuteTimer = GetGameTimer();
        static private long timeSyncCooldown = GetGameTimer();
        static private bool freezeTime = false;

        static public void Init()
        {
            server.RegisterEventHandler("onResourceStart", new Action<string>(OnResourceStart));
            server.RegisterEventHandler("onResourceStop", new Action<string>(OnResourceStop));

            server.RegisterTickHandler(OnTimeTick);
        }

        static void OnResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
        }

        static void OnResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
        }

        static async Task OnTimeTick()
        {
            if (minuteClockSpeed > 2000)
            {
                await Server.Delay(2000);
            }
            else
            {
                await Server.Delay(minuteClockSpeed);
            }
            if (!freezeTime)
            {
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
            }

            if (GetGameTimer() - timeSyncCooldown > 6000)
            {
                Server.TriggerClientEvent("curiosity:Player:World:SetTime", currentHours, currentMinutes, freezeTime);
                timeSyncCooldown = GetGameTimer();
            }
        }
    }
}
