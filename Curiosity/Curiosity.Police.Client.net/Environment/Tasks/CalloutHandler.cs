using Curiosity.Shared.Client.net.Enums.Patrol;
using System;
using System.Threading.Tasks;
using CitizenFX.Core.Native;

namespace Curiosity.Police.Client.net.Environment.Tasks
{
    class CalloutHandler
    {
        static int FIVE_MINUTES = ((1000*60) * 5);
        static Client client = Client.GetInstance();
        static Random random = new Random();
        static int PreviousCallout = 0;

        static long TimeStampOfLastCallout;

        public static void PlayerCanTakeCallout()
        {
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

                if ((API.GetGameTimer() - TimeStampOfLastCallout) > FIVE_MINUTES)
                {
                    if (Job.DutyManager.IsOnCallout)
                        TimeStampOfLastCallout = API.GetGameTimer();

                    await Client.Delay(10000);
                    await Task.FromResult(0);
                    return;
                }

                if (random.Next(1) == 1) // 50/50 chance of being called out to the middle of the map
                {
                    int callout = await GetRandomCalloutNumber(ClassLoader.RuralCallOuts.Count);
                    ClassLoader.RuralCallOuts[callout].Invoke();
                    TimeStampOfLastCallout = API.GetGameTimer();
                    await Task.FromResult(0);
                    return;
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.City)
                {
                    int callout = await GetRandomCalloutNumber(ClassLoader.CityCallOuts.Count);
                    ClassLoader.CityCallOuts[callout].Invoke();
                    TimeStampOfLastCallout = API.GetGameTimer();
                    await Task.FromResult(0);
                    return;
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.Country)
                {
                    int callout = await GetRandomCalloutNumber(ClassLoader.CountryCallOuts.Count);
                    ClassLoader.CountryCallOuts[callout].Invoke();
                    TimeStampOfLastCallout = API.GetGameTimer();
                    await Task.FromResult(0);
                    return;
                }
            }
            catch (Exception ex)
            {
                // nothing
            }
        }

        static async Task<int> GetRandomCalloutNumber(int calloutDictionaryCount)
        {
            int randomCallout = random.Next(0, calloutDictionaryCount - 1);

            while (PreviousCallout == randomCallout)
            {
                randomCallout = random.Next(0, calloutDictionaryCount - 1);
                await Client.Delay(10);
            }
            PreviousCallout = randomCallout;
            return randomCallout;
        }
    }
}
