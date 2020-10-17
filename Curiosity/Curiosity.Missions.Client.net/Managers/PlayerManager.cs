using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using Curiosity.Global.Shared.EventWrapper;
using Curiosity.Missions.Client.Utils;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Missions.Client.Managers
{
    class PlayerManager : BaseScript
    {
        const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";

        static ExportDictionary exportDictionary;
        static PlayerInformationModel playerInfo;
        static public PatrolZone PatrolZone = PatrolZone.City;
        static public Vehicle Vehicle;
        static public Privilege Privilege;

        static public bool IsOnDuty;
        static public bool IsOfficer;

        public PlayerManager()
        {
            EventHandlers[Events.Client.ServerPlayerInformationUpdate] += new Action<string>(OnPlayerInformationUpdate);
            EventHandlers[Events.Client.ReceivePlayerInformation] += new Action<string>(OnPlayerInformationUpdate);
            EventHandlers[Events.Client.PolicePatrolZone] += new Action<int>(OnPlayerPatrolZone);
            EventHandlers[Events.Client.PoliceDutyEvent] += new Action<bool, bool, string>(OnPoliceDuty);
            EventHandlers[Events.Client.CurrentVehicle] += new Action<int>(OnVehicleId);
            EventHandlers[Events.Client.OnClientResourceStart] += new Action<string>(OnClientResourceStart);
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            TriggerEvent("curiosity:Client:Player:Information");
        }

        private static void OnVehicleId(int vehicleId)
        {
            if (Vehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                Vehicle = new Vehicle(vehicleId);
            }
            else if (Vehicle.Handle != vehicleId)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                Vehicle = new Vehicle(vehicleId);
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

        private void OnPlayerPatrolZone(int zone)
        {
            PatrolZone = (PatrolZone)zone;
        }

        private void OnPlayerInformationUpdate(string json)
        {
            playerInfo = JsonConvert.DeserializeObject<PlayerInformationModel>(json);

            if (Enum.TryParse($"{playerInfo.RoleId}", out Privilege))
                playerInfo.Role = Privilege;
        }

        public static bool IsDeveloper => playerInfo.Role == Privilege.DEVELOPER || playerInfo.Role == Privilege.PROJECTMANAGER;
        public static bool IsDeveloperUIActive => (IsDeveloper) && Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG_UI);

        public class PlayerInformationModel
        {
            public string Handle;
            public int UserId;
            public int CharacterId;
            public int RoleId;
            public Privilege Role;
            public int Wallet;
            public int BankAccount;
            public Dictionary<string, Skills> Skills;
        }

        public class Skills
        {
            public int Id;
            public int TypeId;
            public string Description;
            public string Label;
            public string LabelDescription;
            public int Value;
        }
    }
}
