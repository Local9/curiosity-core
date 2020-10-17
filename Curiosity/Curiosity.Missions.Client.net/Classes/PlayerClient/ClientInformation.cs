using CitizenFX.Core;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Extensions;
using System;

namespace Curiosity.Missions.Client.Classes.PlayerClient
{
    static class ClientInformation
    {
        static PluginManager PluginInstance => PluginManager.Instance;
        public static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public static Privilege privilege;
        public static Job ClientJob;

        public static PatrolZone patrolZone;

        public static bool IsPlayerCalloutActive = false;

        public static bool IsPlayerAvailable = false;
        public static bool IsPlayerJobActive = false;

        public static bool IsPlayerOnActiveBackup = false;

        public static void Init()
        {
            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:SetJobActive", new Action<bool, string>(OnSetJobActive));
            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:SetPlayerAvailable", new Action<bool>(OnSetPlayerAvailable));

            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:IsPlayerCalloutActive", new Action(OnIsPlayerCalloutActive));
            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:IsPlayerAvailable", new Action(OnIsPlayerAvailable));
            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:IsPlayerJobActive", new Action(OnIsPlayerJobActive));
            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:IsPlayerOnActiveBackup", new Action(OnIsPlayerOnActiveBackup));

            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:SetLocation", new Action<int>(OnSetMissionLocation));

            PluginInstance.RegisterEventHandler("curiosity:Client:Player:InternalInformation", new Action<string>(PlayerInfo));
            PluginInstance.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            PluginInstance.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
        }

        static void OnSetMissionLocation(int location)
        {
            patrolZone = (PatrolZone)location;
        }

        public static bool IsTrusted()
        {
            return privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER;
        }

        static void OnIsPlayerCalloutActive()
        {
            PluginManager.TriggerEvent("curiosity:Client:Mission:CalloutState", IsPlayerCalloutActive);
        }

        static void OnIsPlayerAvailable()
        {
            PluginManager.TriggerEvent("curiosity:Client:Mission:PlayerAvailable", IsPlayerAvailable);
        }

        static void OnIsPlayerJobActive()
        {
            PluginManager.TriggerEvent("curiosity:Client:Mission:PlayerRole", $"{ClientJob}", IsPlayerJobActive);
        }

        static void OnIsPlayerOnActiveBackup()
        {
            PluginManager.TriggerEvent("curiosity:Client:Mission:PlayerOnActiveBackup", IsPlayerOnActiveBackup);
        }

        static void OnSetJobActive(bool active, string role)
        {
            IsPlayerJobActive = active;
            if (!Enum.TryParse(role, out ClientJob))
            {
                if (Permissions.IsDeveloper(privilege))
                {
                    Log.Error($"OnSetRoleActive -> Invalid Job Role: {role}");
                    Log.Error($"OnSetRoleActive -> Valid Job Roles: {string.Join(", ", EnumExtension.GetListOfDescription<Job>())}");
                    Log.Notification("ERROR", "Invalid Job Role", "More information in developer F8 console.");
                }
            }
        }

        static void OnSetPlayerAvailable(bool available) // Allow player to go on break
        {
            IsPlayerAvailable = available;
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

            PluginManager.TriggerEvent("curiosity:Client:Player:Information");
        }

        static void OnPlayerSpawned()
        {
            PluginManager.TriggerEvent("curiosity:Client:Player:Information");
        }


        static async void PlayerInfo(string json)
        {
            playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);

            if (privilege != (Privilege)playerInfo.RoleId)
            {
                privilege = (Privilege)playerInfo.RoleId;
            }

            await BaseScript.Delay(0);
        }
    }
}
