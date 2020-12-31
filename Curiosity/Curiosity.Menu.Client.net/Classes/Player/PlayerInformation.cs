using CitizenFX.Core;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using System;

namespace Curiosity.Menus.Client.net.Classes.Player
{
    static class PlayerInformation
    {
        static Client client = Client.GetInstance();
        public static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public static Privilege privilege;
        public static Job Job;
        public static bool IsOnDuty;

        private static bool startup;

        public static async void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Player:GetInformation", new Action<string>(PlayerInfo));
            client.RegisterEventHandler("curiosity:Client:Player:InternalInformation", new Action<string>(PlayerInfo));

            await Client.Delay(2000);

            Client.TriggerEvent("curiosity:Client:Player:Information");
        }

        static async void PlayerInfo(string json)
        {
            playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);

            if (privilege != (Privilege)playerInfo.RoleId)
            {
                privilege = (Privilege)playerInfo.RoleId;
                Menus.Donator.UpdateSubmenu();
            }

            if (!startup)
            {
                startup = true;
                // MENU 2.0
                Menus.MenuBase.Init();
                Menus.Donator.Init();
            }

            await BaseScript.Delay(0);
        }

        public static bool IsDonator()
        {
            return (privilege == Privilege.DONATOR_LIFE || privilege == Privilege.DONATOR_LEVEL_1 || privilege == Privilege.DONATOR_LEVEL_2 || privilege == Privilege.DONATOR_LEVEL_3);
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
    }
}
