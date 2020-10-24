using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.MissionManager.Shared.Classes;
using Curiosity.MissionManager.Shared.EventWrapper;
using Newtonsoft.Json;
using System;

namespace Curiosity.MissionManager.Client.Environment
{
    class CurPlayer : BaseScript
    {
        const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";

        internal static ExportDictionary exportDictionary;

        internal static PlayerInformationModel playerInfo = new PlayerInformationModel();
        internal static Privilege privilege;
        internal static PatrolZone PatrolZone = PatrolZone.City;
        internal static Vehicle PersonalVehicle;
        internal static Vehicle PersonalTrailer;

        internal static bool IsOnDuty;
        internal static bool IsOfficer;

        public CurPlayer()
        {
            EventHandlers[Events.Client.ReceivePlayerInformation] += new Action<string>(OnPlayerInformationUpdate);

            EventHandlers[Events.Client.PolicePatrolZone] += new Action<int>(OnPlayerPatrolZone);
            EventHandlers[Events.Client.PoliceDutyEvent] += new Action<bool, bool, string>(OnPoliceDuty);
            EventHandlers[Events.Client.CurrentVehicle] += new Action<int>(OnVehicleId);

            EventHandlers[Events.Native.Client.PlayerSpawned] += new Action<dynamic>(OnPlayerSpawned);
            EventHandlers[Events.Native.Client.OnClientResourceStart.Path] += Events.Native.Client.OnClientResourceStart.Action += OnClientResourceStart;
        }

        private static void OnVehicleId(int vehicleId)
        {
            if (PersonalVehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                PersonalVehicle = new Vehicle(vehicleId);
            }
            else if (PersonalVehicle.Handle != vehicleId)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                PersonalVehicle = new Vehicle(vehicleId);
            }
        }

        private void OnPoliceDuty(bool active, bool onduty, string job)
        {
            IsOnDuty = onduty;
            IsOfficer = (job == "police");

            if (IsOfficer)
            {
                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Cop;
            }
            else
            {
                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Player;
            }
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            int vehicleId = API.GetResourceKvpInt(PERSONAL_VEHICLE_KEY);

            if (vehicleId > 0)
            {
                Vehicle kvpVehicle = new Vehicle(vehicleId);
                if (kvpVehicle.Exists())
                {
                    if (!kvpVehicle.IsDead)
                        PersonalVehicle = kvpVehicle;
                }
            }

            BaseScript.TriggerEvent("curiosity:Client:Player:Information");
        }

        private void OnPlayerSpawned(dynamic spawnObject)
        {
            BaseScript.TriggerEvent("curiosity:Client:Player:Information");
        }

        private void OnPlayerInformationUpdate(string jsonString)
        {
            playerInfo = JsonConvert.DeserializeObject<PlayerInformationModel>(jsonString);
            privilege = (Privilege)playerInfo.RoleId;
        }

        private void OnPlayerPatrolZone(int zone)
        {
            PatrolZone = (PatrolZone)zone;
        }

        internal static bool IsDeveloper => privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER;
        internal static bool IsDonator => privilege == Privilege.DONATOR || privilege == Privilege.DONATOR1 || privilege == Privilege.DONATOR2 || privilege == Privilege.DONATOR3;
        internal static bool IsStaff => privilege == Privilege.HEADADMIN || privilege == Privilege.SENIORADMIN || privilege == Privilege.MODERATOR || privilege == Privilege.ADMINISTRATOR || privilege == Privilege.COMMUNITYMANAGER;
    }
}
