using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core.Native;

namespace Curiosity.Police.Client.net.Environment.Tasks
{
    class CalloutHandler
    {
        static int FIVE_MINUTES = ((1000 * 60) * 3);
        // static int FIVE_MINUTES = 15000; // DEV USE
        static Client client = Client.GetInstance();
        static Random random = new Random();
        static int PreviousCallout = -1;
        static bool IsRunnningCallout = false;

        static long TimeStampOfLastCallout;

        public static async void PlayerCanTakeCallout()
        {
            //if (Classes.Player.PlayerInformation.IsDeveloper())
            //    Log.Verbose($"TICK: PlayerCanTakeCallout");

            await Client.Delay(0);
            IsRunnningCallout = false;
            TimeStampOfLastCallout = API.GetGameTimer();
            client.RegisterTickHandler(SelectCallout);
        }

        public static async void PlayerIsOnActiveCalloutOrOffDuty()
        {
            //if (Classes.Player.PlayerInformation.IsDeveloper())
            //    Log.Verbose($"TICK: PlayerIsOnActiveCalloutOrOffDuty");

            await Client.Delay(0);
            client.DeregisterTickHandler(SelectCallout);
        }

        static async Task SelectCallout()
        {
            try
            {
                if (!Job.DutyManager.IsPoliceJobActive)
                {
                    await Task.FromResult(0);
                    return;
                }

                if ((API.GetGameTimer() - TimeStampOfLastCallout) < FIVE_MINUTES)
                {
                    if (Job.DutyManager.IsOnCallout)
                    {
                        TimeStampOfLastCallout = API.GetGameTimer();
                    }

                    //if (Classes.Player.PlayerInformation.IsDeveloper())
                    //    Log.Verbose($"SelectCallout TimerValue: {(API.GetGameTimer() - TimeStampOfLastCallout) / 1000.0}s");

                    await Client.Delay(10000);
                    await Task.FromResult(0);
                    return;
                }

                if (IsRunnningCallout)
                {
                    await Task.FromResult(0);
                    return;
                };

                IsRunnningCallout = true;

                await Task.FromResult(0);

                if (random.Next(1) == 1) // 50/50 chance of being called out to the middle of the map
                {
                    GetRandomCallout(ClassLoader.RuralCallOuts);
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.City)
                {

                    GetRandomCallout(ClassLoader.CityCallOuts);
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.Country)
                {
                     GetRandomCallout(ClassLoader.CountryCallOuts);
                }
            }
            catch (Exception ex)
            {
                // nothing
            }
        }

        static async void GetRandomCallout(Dictionary<int, Func<bool>> calloutDictionary)
        {
            //if (Classes.Player.PlayerInformation.IsDeveloper())
            //    Log.Verbose($"GetRandomCallout");

            int randomCalloutIndex = random.Next(0, calloutDictionary.Count);
            int calloutId = calloutDictionary.ElementAt(randomCalloutIndex).Key;

            bool foundNewCallout = false;

            while (!foundNewCallout)
            {
                randomCalloutIndex = random.Next(0, calloutDictionary.Count);
                calloutId = calloutDictionary.ElementAt(randomCalloutIndex).Key;

                if (PreviousCallout != calloutId)
                {
                    foundNewCallout = true;
                }

                await Client.Delay(100);
            }

            PreviousCallout = calloutId;

            //if (Classes.Player.PlayerInformation.IsDeveloper())
            //    Log.Verbose($"GetRandomCallout: Result {calloutId}");

            calloutDictionary.ElementAt(randomCalloutIndex).Value.Invoke();

            await Task.FromResult(0);
        }
    }
}
