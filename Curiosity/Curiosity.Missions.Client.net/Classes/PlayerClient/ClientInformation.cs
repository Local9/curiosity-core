using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Global.Shared.net.Data;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using System;

namespace Curiosity.Missions.Client.net.Classes.PlayerClient
{
    static class ClientInformation
    {
        static Client client = Client.GetInstance();
        public static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public static Privilege privilege;
        public static Job ClientJob;

        public static bool IsPlayerCalloutActive = false;

        public static bool IsPlayerAvailable = false;
        public static bool IsPlayerJobActive = false;

        public static bool IsPlayerOnActiveBackup = false;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Mission:SetJobActive", new Action<bool, string>(OnSetJobActive));
            client.RegisterEventHandler("curiosity:Client:Mission:SetPlayerAvailable", new Action<bool>(OnSetPlayerAvailable));

            client.RegisterEventHandler("curiosity:Client:Mission:IsPlayerCalloutActive", new Action(OnIsPlayerCalloutActive));
            client.RegisterEventHandler("curiosity:Client:Mission:IsPlayerAvailable", new Action(OnIsPlayerAvailable));
            client.RegisterEventHandler("curiosity:Client:Mission:IsPlayerJobActive", new Action(OnIsPlayerJobActive));
            client.RegisterEventHandler("curiosity:Client:Mission:IsPlayerOnActiveBackup", new Action(OnIsPlayerOnActiveBackup));

            client.RegisterEventHandler("curiosity:Client:Player:InternalInformation", new Action<string>(PlayerInfo));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
        }

        static void OnIsPlayerCalloutActive()
        {
            Client.TriggerEvent("curiosity:Client:Mission:CalloutState", IsPlayerCalloutActive);
        }

        static void OnIsPlayerAvailable()
        {
            Client.TriggerEvent("curiosity:Client:Mission:PlayerAvailable", IsPlayerAvailable);
        }

        static void OnIsPlayerJobActive()
        {
            Client.TriggerEvent("curiosity:Client:Mission:PlayerRole", $"{ClientJob}", IsPlayerJobActive);
        }

        static void OnIsPlayerOnActiveBackup()
        {
            Client.TriggerEvent("curiosity:Client:Mission:PlayerOnActiveBackup", IsPlayerOnActiveBackup);
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

            Client.TriggerEvent("curiosity:Client:Player:Information");
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
