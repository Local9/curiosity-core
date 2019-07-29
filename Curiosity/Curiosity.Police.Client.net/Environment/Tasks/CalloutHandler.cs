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
        static int FIVE_MINUTES = ((1000*60) * 5);
        static Client client = Client.GetInstance();
        static Random random = new Random();
        static int PreviousCallout = -1;

        static long TimeStampOfLastCallout;

        public static void PlayerCanTakeCallout()
        {
            TimeStampOfLastCallout = API.GetGameTimer();
            
            client.RegisterTickHandler(SelectCallout);
        }

        public static void PlayerIsOnActiveCalloutOrOffDuty()
        {
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

                    await Client.Delay(10000);
                    await Task.FromResult(0);
                    return;
                }

                await Task.FromResult(0);

                Func<bool> CallOutToInvoke = null;

                if (random.Next(1) == 1) // 50/50 chance of being called out to the middle of the map
                {
                    CallOutToInvoke = await GetRandomCallout(ClassLoader.RuralCallOuts);
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.City)
                {

                    CallOutToInvoke = await GetRandomCallout(ClassLoader.CityCallOuts);
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.Country)
                {
                    CallOutToInvoke = await GetRandomCallout(ClassLoader.CountryCallOuts);
                }

                if (CallOutToInvoke == null) return;

                await Client.Delay(0);

                CallOutToInvoke.Invoke();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                // nothing
            }
        }

        static async Task<Func<bool>> GetRandomCallout(Dictionary<int, Func<bool>> calloutDictionary)
        {
            int calloutId = random.Next(0, calloutDictionary.Count);

            while (PreviousCallout == calloutId)
            {
                calloutId = calloutDictionary.ElementAt(calloutId).Key;
                await Client.Delay(0);
            }
            PreviousCallout = calloutId;

            return calloutDictionary.ElementAt(calloutId).Value;
        }
    }
}
