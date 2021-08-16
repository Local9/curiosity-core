using Curiosity.Core.Server.Diagnostics;
using Curiosity.Systems.Library.Events;
using Curiosity.Core.Server.Events;
using System;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class OneSyncEventManager : Manager<OneSyncEventManager>
    {
        /* // https://cookbook.fivem.net/2020/11/27/routing-buckets-split-game-state/ //
         * 
         * playerEnteredScope
         * playerLeftScope
         * entityCreated
         * 
         * Player Scope could be used for collision updating?
         * 
         * */

        List<int> requestedRightsToSpawn = new List<int>();

        public override void Begin()
        {
            Logger.Debug($"[INIT] OneSyncEventManager");
            Instance.EventRegistry.Add("entityCreating", new Action<int>(OnEntityCreating));
            // Instance.EventRegistry.Add("entityCreated", new Action<int>(OnEntityCreated));

            EventSystem.GetModule().Attach("onesync:request", new EventCallback(metadata => {
                if (requestedRightsToSpawn.Contains(metadata.Sender))
                    return true;

                if (!requestedRightsToSpawn.Contains(metadata.Sender))
                {
                    requestedRightsToSpawn.Add(metadata.Sender);
                    return true;
                }

                return false;
            }));
        }

        private void OnEntityCreated(int handle)
        {
            if (DoesEntityExist(handle))
            {
                int owner = NetworkGetEntityOwner(handle);
                if (requestedRightsToSpawn.Contains(owner))
                {
                    requestedRightsToSpawn.Remove(owner);
                }
            }
        }

        private void OnEntityCreating(int handle)
        {
            if (DoesEntityExist(handle))
            {
                int populationType = GetEntityPopulationType(handle);
                if (populationType > 10)
                {
                    Logger.Info($"Population Type {populationType} is not known");
                }

                PopulationType population = (PopulationType)populationType;
                
                if (population == PopulationType.MISSION)
                {
                    int owner = NetworkGetEntityOwner(handle);

                    if (PluginManager.PlayersList[owner] is not null || requestedRightsToSpawn.Contains(owner))
                    {
                        if (requestedRightsToSpawn.Contains(owner))
                            requestedRightsToSpawn.Remove(owner);
                        return;
                    }

                    if (!requestedRightsToSpawn.Contains(owner))
                    {
                        DeleteEntity(handle);
                        CancelEvent();
                    }
                }
            }
        }
    }

    enum PopulationType
    {
        UNKNOWN,
        RANDOM_PERMANENT,
        RANDOM_PARKED,
        RANDOM_PATROL,
        RANDOM_SCENARIO,
        RANDOM_AMBIENT,
        PERMANENT,
        MISSION,
        REPLAY,
        CACHE,
        TOOL
    }
}
