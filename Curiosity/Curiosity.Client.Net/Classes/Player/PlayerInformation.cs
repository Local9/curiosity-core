using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Extensions;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Client.net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Player
{
    static class PlayerInformation
    {
        static Client client = Client.GetInstance();
        public static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public static Privilege privilege;
        static bool statsSet = false;
        static List<int> listOfIncidents = new List<int>();

        public static async void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Player:GetInformation", new Action<string>(PlayerInfo));
            client.RegisterEventHandler("curiosity:Client:Player:Information", new Action(GetPlayerInfo));
            client.RegisterEventHandler("curiosity:Client:Player:UpdateFlags", new Action(OnFlagUpdate));
            client.RegisterEventHandler("curiosity:Client:Player:UpdateExtraFlags", new Action(UpdateExtraFlags));
            client.RegisterEventHandler("curiosity:Client:Player:Developer:Online", new Action(DeveloperOnline));

            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(SetDutyIcon));

            await BaseScript.Delay(1000);
            PeriodicCheck();
        }

        public static int GetTotalEarnings()
        {
            return playerInfo.Wallet + playerInfo.BankAccount;
        }

        static void SetDutyIcon(bool onDuty, bool active, string job)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new DutyMessage { duty = onDuty, dutyActive = active, job = job }));
        }

        static void DeveloperOnline()
        {
            ForceLightningFlash();
        }

        static void UpdateExtraFlags()
        {
            SetPoliceRadarBlips(false);
            int outIncidentId = 0;
            if (CreateIncidentWithEntity(14, Game.PlayerPed.Handle, 3, 3f, ref outIncidentId))
            {
                SetIncidentRequestedUnits(outIncidentId, 14, 3);

                if (outIncidentId > 0)
                {
                    listOfIncidents.Add(outIncidentId);
                }
            }
            int outIncidentId2 = 0;
            if (CreateIncidentWithEntity(11, Game.PlayerPed.Handle, 6, 3f, ref outIncidentId2))
            {
                SetIncidentRequestedUnits(outIncidentId2, 11, 6);

                if (outIncidentId2 > 0)
                {
                    listOfIncidents.Add(outIncidentId2);
                }
            }
        }

        static void OnFlagUpdate()
        {
            SetPoliceRadarBlips(false);
            int outIncidentId = 0;
            if (CreateIncidentWithEntity(14, Game.PlayerPed.Handle, 3, 3f, ref outIncidentId))
            {
                SetIncidentRequestedUnits(outIncidentId, 14, 3);

                if (outIncidentId > 0)
                {
                    listOfIncidents.Add(outIncidentId);
                }

                client.RegisterTickHandler(OnPlayerDeathCheck);
            }
        }

        static async Task OnPlayerDeathCheck()
        {
            while (Game.PlayerPed.IsAlive)
            {
                await BaseScript.Delay(100);
            }

            listOfIncidents.ForEach(incidentId =>
            {
                DeleteIncident(incidentId);
            });

            listOfIncidents.Clear();

            client.DeregisterTickHandler(OnPlayerDeathCheck);
        }

        static async void GetPlayerInfo()
        {
            Client.TriggerEvent("curiosity:Client:Player:InternalInformation", Newtonsoft.Json.JsonConvert.SerializeObject(playerInfo));
            await BaseScript.Delay(0);
        }

        static async void PlayerInfo(string json)
        {
            playerInfo = JsonConvert.DeserializeObject<PlayerInformationModel>(json);

            UiPlayerCharacter uiPlayer = new UiPlayerCharacter();
            uiPlayer.UserId = playerInfo.UserId;
            uiPlayer.Username = Game.Player.Name;
            uiPlayer.Finance.Cash = playerInfo.Wallet;
            uiPlayer.Finance.Bank = playerInfo.BankAccount;

            string nuiData = JsonConvert.SerializeObject(uiPlayer);
            API.SendNuiMessage(nuiData);

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

            if (Game.PlayerPed != null)
                Decorators.Set(Game.PlayerPed.Handle, Decorators.DECOR_PLAYER_STAFF, IsStaff());

            GetPlayerInfo();
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

    class RequiresPermissionFlags : Attribute
    {
        public RequiresPermissionFlags(Privilege privilege)
        {
            if (Player.PlayerInformation.privilege != privilege)
                throw new SecurityException("You don't have the access rights to perform this action");
        }
    }
}
