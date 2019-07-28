using System;
using Curiosity.Shared.Client.net.Enums.Patrol;
using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Job
{
    class DutyManager
    {
        static Client client = Client.GetInstance();

        public static bool IsOnDuty = false;
        public static bool IsPoliceJobActive = false;
        public static bool IsOnCallout = false;
        public static PatrolZone PatrolZone = PatrolZone.City;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));
            client.RegisterEventHandler("curiosity:Client:Police:PatrolZone", new Action<int>(OnPatrolZone));
            client.RegisterEventHandler("curiosity:Client:Police:SetCallOutStatus", new Action<bool>(OnSetCallOutStatus));
            client.RegisterEventHandler("curiosity:Client:Police:GetCallOutStatus", new Action(OnGetCallOutStatus));
        }

        static void OnDutyState(bool jobActive, bool dutyState, string job)
        {
            if (job != "police")
            {
                IsPoliceJobActive = false;
                return; // TODO: Refactor job code
            }
            IsPoliceJobActive = true;
            IsOnDuty = dutyState;
        }

        static void OnPatrolZone(int zone)
        {
            if (!IsPoliceJobActive) return;
            PatrolZone = (PatrolZone)zone;
        }

        static void OnSetCallOutStatus(bool status)
        {
            if (!IsPoliceJobActive) return;
            IsOnCallout = status;
        }

        static void OnGetCallOutStatus()
        {
            if (!IsPoliceJobActive) return;
            Client.TriggerEvent("curiosity:Client:Police:CallOutStatus", IsOnCallout);
        }
    }
}
