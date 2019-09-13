using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using System;

namespace Curiosity.Client.net.Classes.Player
{
    static class PlayerInformation
    {
        static Client client = Client.GetInstance();
        public static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public static Privilege privilege;
        static bool statsSet = false;

        public static async void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Player:GetInformation", new Action<string>(PlayerInfo));
            client.RegisterEventHandler("curiosity:Client:Player:Information", new Action(GetPlayerInfo));
            await BaseScript.Delay(1000);
            PeriodicCheck();
        }

        static async void GetPlayerInfo()
        {
            Client.TriggerEvent("curiosity:Client:Player:InternalInformation", Newtonsoft.Json.JsonConvert.SerializeObject(playerInfo));
            await BaseScript.Delay(0);
        }

        static async void PlayerInfo(string json)
        {
            playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);

            privilege = (Privilege)playerInfo.RoleId;

            if (privilege == Privilege.DEVELOPER && !statsSet)
            {
                statsSet = true;
                StatSetInt((uint)GetHashKey("MP0_STAMINA"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_STRENGTH"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_LUNG_CAPACITY"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_WHEELIE_ABILITY"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_FLYING_ABILITY"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_SHOOTING_ABILITY"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_STEALTH_ABILITY"), 100, true);
            }

            await BaseScript.Delay(0);
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

        public static bool IsProjectManager()
        {
            return (privilege == Privilege.PROJECTMANAGER);
        }

        static private async void PeriodicCheck()
        {
            await BaseScript.Delay(5000);
            while (true)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:GetInformation");
                await BaseScript.Delay(60000);
            }
        }
    }
}
