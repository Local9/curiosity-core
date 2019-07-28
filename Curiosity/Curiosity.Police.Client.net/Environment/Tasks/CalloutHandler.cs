using Curiosity.Shared.Client.net.Enums.Patrol;
using System;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.net.Environment.Tasks
{
    class CalloutHandler
    {
        static Client client = Client.GetInstance();
        static Random random = new Random();
        static int currentCallout;

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
            if (!Job.DutyManager.IsPoliceJobActive)
            {
                await Task.FromResult(0);
                return;
            }

            if (Job.DutyManager.PatrolZone == PatrolZone.City)
            {
                int randomCallout = random.Next(0, ClassLoader.CityCallOuts.Count);
                ClassLoader.CityCallOuts[randomCallout].Invoke();
                await Task.FromResult(0);
                return;
            }
        }
    }
}
