using System;

namespace Curiosity.Interface.Client.Managers
{
    public class PlayerSpawnedManager : Manager<PlayerSpawnedManager>
    {
        public override void Begin()
        {
            Curiosity.EventRegistry["onPlayerSpawned"] += new Action<dynamic>(OnPlayerSpawned);
        }

        private void OnPlayerSpawned(dynamic obj)
        {
            Session.HasSpawned = true;
        }
    }
}
