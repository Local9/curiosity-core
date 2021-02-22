using CitizenFX.Core.Native;
using System;

namespace Curiosity.Interface.Client.Managers
{
    public class PlayerSpawnedManager : Manager<PlayerSpawnedManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry["playerSpawned"] += new Action<dynamic>(OnPlayerSpawned);
            Instance.EventRegistry["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            Instance.EventRegistry["onResourceStop"] += new Action<string>(OnClientResourceStop);

            Instance.ExportRegistry.Add("SessionActive", new Func<bool>(() => { Session.HasSpawned = true; return true; }));
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
        }

        private void OnPlayerSpawned(dynamic obj)
        {
            Session.HasSpawned = true;
        }
    }
}
