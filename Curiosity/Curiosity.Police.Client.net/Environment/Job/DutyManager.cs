using System;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.net.Environment.Job
{
    class DutyManager
    {
        static Client client = Client.GetInstance();

        public static bool IsOnDuty = false;
        public static bool IsPoliceJobActive = false;
        public static bool IsOnCallout = false;
        public static PatrolZone PatrolZone = PatrolZone.City;

        public static RelationshipGroup PoliceRelationshipGroup;

        public static void Init()
        {
            PoliceRelationshipGroup = World.AddRelationshipGroup("POLICE_CALLOUTS");

            API.RegisterCommand("duty", new Action(OnPoliceDuty), false);

            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));
            client.RegisterEventHandler("curiosity:Client:Police:PatrolZone", new Action<int>(OnPatrolZone));
        }

        static void OnPoliceDuty()
        {
            if (!Classes.Player.PlayerInformation.IsDeveloper()) return;

            Client.TriggerEvent("curiosity:Client:Interface:Duty", true, false, "police");
        }

        static void OnDutyState(bool jobActive, bool dutyState, string job)
        {
            if (Classes.Player.PlayerInformation.IsDeveloper())
            {
                Log.Info($"OnDutyState -> Method Called");
            }

            if (job == "error")
            {
                Client.TriggerServerEvent("curiosity:Server:Player:Job", (int)Curiosity.Global.Shared.net.Enums.Job.Unknown);
                client.DeregisterTickHandler(Classes.Menus.PoliceDispatchMenu.OnTaskKeyCombination);
                Game.PlayerPed.Weapons.RemoveAll();

                Log.Error("OnDutyState -> Error");
                IsPoliceJobActive = false;
                IsOnDuty = false;
                IsOnCallout = false;
                // Tasks.CalloutHandler.PlayerIsOnActiveCalloutOrOffDuty();
                return;
            }

            if (job != "police")
            {
                Client.TriggerServerEvent("curiosity:Server:Player:Job", (int)Curiosity.Global.Shared.net.Enums.Job.Unknown);
                client.DeregisterTickHandler(Classes.Menus.PoliceDispatchMenu.OnTaskKeyCombination);

                Game.PlayerPed.Weapons.RemoveAll();

                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info($"OnDutyState -> Job changed from police to {job}");
                }
                IsPoliceJobActive = false;
                IsOnDuty = false;
                IsOnCallout = false;
                return; // TODO: Refactor job code
            }

            Client.TriggerEvent("curiosity:Client:Context:ShowDutyMenu", false, string.Empty, string.Empty);
            Client.TriggerServerEvent("curiosity:Server:Player:Job", (int)Curiosity.Global.Shared.net.Enums.Job.Police);
            Game.PlayerPed.IsInvincible = false;

            client.RegisterTickHandler(Classes.Menus.PoliceDispatchMenu.OnTaskKeyCombination);

            IsOnDuty = dutyState;

            if (Classes.Player.PlayerInformation.IsDeveloper())
            {
                Log.Info($"OnDutyState -> Player Duty State {IsOnDuty}");
            }
            Game.PlayerPed.RelationshipGroup = PoliceRelationshipGroup;
            Game.PlayerPed.DropsWeaponsOnDeath = false;

            IsPoliceJobActive = true;

            if (IsOnDuty)
            {
                Client.TriggerEvent("curiosity:Client:Context:ShowDutyMenu", true, "Resupply Ammo", "curiosity:Player:Loadout:Resupply");
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
        }

        static void OnGetCallOutStatus()
        {
            if (!IsPoliceJobActive) return;
            Client.TriggerEvent("curiosity:Client:Police:CallOutStatus", IsOnCallout);
        }
    }
}
