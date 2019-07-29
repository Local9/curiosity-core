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

                if ((API.GetGameTimer() - TimeStampOfLastCallout) < FIVE_MINUTES)
                {
                    await Client.Delay(10000);
                    await Task.FromResult(0);
                    return;
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.City)
                {
                    int randomCallout = random.Next(0, ClassLoader.CityCallOuts.Count - 1);

                    while (PreviousCallout == randomCallout)
                    {
                        randomCallout = randomCallout = random.Next(0, ClassLoader.CityCallOuts.Count - 1);
                        await Client.Delay(10);
                    }
                    PreviousCallout = randomCallout;

                    ClassLoader.CityCallOuts[randomCallout].Invoke();

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
    }
}
