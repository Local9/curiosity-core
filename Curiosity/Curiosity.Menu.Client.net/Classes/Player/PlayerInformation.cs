using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
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

            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));

            await Client.Delay(2000);

            Client.TriggerEvent("curiosity:Client:Player:Information");
        }

        static void OnDutyState(bool jobActive, bool dutyState, string job)
        {
            if (job == "error")
            {
                IsOnDuty = false;
                return;
            }

            if (job == "police")
            {
                Job = Job.Police;
            }
            else
            {
                Job = Job.Unknown;
            }
        }

        static async void PlayerInfo(string json)
        {
            playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);

            if (privilege != (Privilege)playerInfo.RoleId)
            {
                privilege = (Privilege)playerInfo.RoleId;
            }

            if (!startup)
            {
                startup = true;
                // MENU 2.0
                Classes.Menus.MenuBase.Init();
                Classes.Menus.Donator.Init();
            }

            await BaseScript.Delay(0);
        }

        public static bool IsDonator()
        {
            return (privilege == Privilege.DONATOR);
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
