using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.Interface.Client.Managers
{
    public class PlayerSpawnedManager : Manager<PlayerSpawnedManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry["playerSpawned"] += new Action<dynamic>(OnPlayerSpawned);
            Instance.EventRegistry["curiosity:Client:Player:SessionActivated"] += new Action<bool, bool, int>(OnSessionActive);

            Instance.EventRegistry["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            Instance.EventRegistry["onResourceStop"] += new Action<string>(OnClientResourceStop);
        }

        private void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            PdaManager.PdaInstance.IsCoreOpen = false;
            PdaManager.PdaInstance.SendPanelMessage();
            PdaManager.PdaInstance.CloseTablet();
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            BaseScript.TriggerServerEvent("curiosity:Server:Player:IsActive");
        }

        private void OnPlayerSpawned(dynamic obj)
        {
            Session.HasSpawned = true;
        }
        private void OnSessionActive(bool showBlips, bool showLocation, int afkMinutes)
        {
            Session.HasSpawned = true;
        }
    }
}
