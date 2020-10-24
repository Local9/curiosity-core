using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net;
using Newtonsoft.Json;
using System;

namespace Curiosity.MissionManager.Client.Environment
{
    class PlayerInformation : BaseScript
    {
        internal static PlayerInformationModel playerInfo = new PlayerInformationModel();
        internal static Privilege privilege;

        public PlayerInformation()
        {
            EventHandlers["curiosity:Client:Player:InternalInformation"] += new Action<string>(OnInternalPlayerInformation);

            EventHandlers["playerSpawned"] += new Action<dynamic>(OnPlayerSpawned);
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            BaseScript.TriggerEvent("curiosity:Client:Player:Information");
        }

        private void OnPlayerSpawned(dynamic spawnObject)
        {
            BaseScript.TriggerEvent("curiosity:Client:Player:Information");
        }

        private void OnInternalPlayerInformation(string jsonString)
        {
            // Log.Verbose("[PlayerInformation] Update");

            playerInfo = JsonConvert.DeserializeObject<PlayerInformationModel>(jsonString);
            privilege = (Privilege)playerInfo.RoleId;
        }

        internal static bool IsDeveloper => privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER;
        internal static bool IsDonator => privilege == Privilege.DONATOR || privilege == Privilege.DONATOR1 || privilege == Privilege.DONATOR2 || privilege == Privilege.DONATOR3;
        internal static bool IsStaff => privilege == Privilege.HEADADMIN || privilege == Privilege.SENIORADMIN || privilege == Privilege.MODERATOR || privilege == Privilege.ADMINISTRATOR || privilege == Privilege.COMMUNITYMANAGER;
    }
}
