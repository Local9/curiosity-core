﻿using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using System;

namespace Curiosity.Vehicles.Client.net.Classes.CurPlayer
{
    static class PlayerInformation
    {
        static Plugin client = Plugin.GetInstance();
        public static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public static Privilege privilege;

        static bool IsSetup = false;

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
            client.RegisterEventHandler("curiosity:Client:Player:GetInformation", new Action<string>(PlayerInfo));
            client.RegisterEventHandler("curiosity:Client:Player:InternalInformation", new Action<string>(PlayerInfo));
        }
        static void OnPlayerSpawned()
        {
            Plugin.TriggerEvent("curiosity:Client:Player:Information");
        }

        static void PlayerInfo(string json)
        {
            playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);
            privilege = (Privilege)playerInfo.RoleId;

            if (privilege == Privilege.DEVELOPER && !IsSetup)
            {
                Screen.ShowNotification("~g~Vehicle Playerinfo Received");
                IsSetup = true;
            }
        }

        public static bool IsDonator()
        {
            return (privilege == Privilege.DONATOR || privilege == Privilege.DONATOR1 || privilege == Privilege.DONATOR2 || privilege == Privilege.DONATOR3);
        }

        public static bool IsStaff()
        {
            return (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.MODERATOR || privilege == Privilege.ADMINISTRATOR || privilege == Privilege.SENIORADMIN || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        }

        public static bool IsAdmin()
        {
            return (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.ADMINISTRATOR || privilege == Privilege.SENIORADMIN || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        }

        public static bool IsTrustedAdmin()
        {
            return (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        }

        public static bool IsDeveloper()
        {
            return (privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        }
    }
}
