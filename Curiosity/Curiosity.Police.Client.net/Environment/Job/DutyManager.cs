using System;

namespace Curiosity.Police.Client.net.Environment.Job
{
    class DutyManager
    {
        static Client client = Client.GetInstance();

        public static bool IsOnDuty = false;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));
        }

        static void OnDutyState(bool jobActive, bool dutyState, string job)
        {
            if (job != "police") return; // TODO: Refactor job code
            IsOnDuty = dutyState;
        }
    }
}
