using System;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net;
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
            if (Classes.Player.PlayerInformation.IsDeveloper())
            {
                Log.Info($"OnDutyState -> Method Called");
            }

            if (job == "error")
            {
                Log.Error("OnDutyState -> Error");
                IsPoliceJobActive = false;
                IsOnDuty = false;
                IsOnCallout = false;
                Tasks.CalloutHandler.PlayerIsOnActiveCalloutOrOffDuty();
                return;
            }

            if (job != "police")
            {
                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info($"OnDutyState -> Job changed from police to {job}");
                }
                IsPoliceJobActive = false;
                IsOnDuty = false;
                if (IsOnCallout)
                {
                    Classes.CreateShopCallout.EndCallout();
                }
                IsOnCallout = false;
                Tasks.CalloutHandler.PlayerIsOnActiveCalloutOrOffDuty();
                return; // TODO: Refactor job code
            }
            IsOnDuty = dutyState;

            if (Classes.Player.PlayerInformation.IsDeveloper())
            {
                Log.Info($"OnDutyState -> Player Duty State {IsOnDuty}");
            }

            if (IsOnDuty)
            {
                IsPoliceJobActive = true;
                Tasks.CalloutHandler.PlayerCanTakeCallout();

                Game.PlayerPed.Weapons.Give(WeaponHash.Pistol, 120, false, true);
                Game.PlayerPed.Armor = 100;
            }
            else
            {
                if (IsOnCallout)
                {
                    Classes.CreateShopCallout.EndCallout();
                }
                Game.PlayerPed.Weapons.RemoveAll();
                IsPoliceJobActive = false;
                Tasks.CalloutHandler.PlayerIsOnActiveCalloutOrOffDuty();
            }
        }

        static void OnPatrolZone(int zone)
        {
            PatrolZone = (PatrolZone)zone;
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Patrol Zone Change", $"Zone: {PatrolZone}", string.Empty, 2);
        }

        public static void OnSetCallOutStatus(bool status)
        {
            if (!IsPoliceJobActive)
            {
                IsOnCallout = false;
                return;
            }

            IsOnCallout = status;

            if (IsOnCallout)
            {
                Tasks.CalloutHandler.PlayerIsOnActiveCalloutOrOffDuty();
            }
            else
            {
                Tasks.CalloutHandler.PlayerCanTakeCallout();
            }
        }

        static void OnGetCallOutStatus()
        {
            if (!IsPoliceJobActive) return;
            Client.TriggerEvent("curiosity:Client:Police:CallOutStatus", IsOnCallout);
        }
    }
}
