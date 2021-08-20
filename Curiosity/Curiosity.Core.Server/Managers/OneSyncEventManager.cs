﻿using Curiosity.Core.Server.Diagnostics;
using Curiosity.Systems.Library.Events;
using Curiosity.Core.Server.Events;
using System;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;
using CitizenFX.Core;
using Curiosity.Systems.Library.Enums;
using System.Threading.Tasks;

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

        Queue<int> queueEntitysCreated = new Queue<int>();

        public override void Begin()
        {
            Logger.Debug($"[INIT] OneSyncEventManager");
            // Instance.EventRegistry.Add("entityCreating", new Action<int>(OnEntityCreating));
            // Instance.EventRegistry.Add("entityCreated", new Action<int>(OnEntityCreated));

            EventSystem.GetModule().Attach("onesync:request", new EventCallback(metadata => {
                return true;

                //if (requestedRightsToSpawn.Contains(metadata.Sender))
                //    return true;

                //if (!requestedRightsToSpawn.Contains(metadata.Sender))
                //{
                //    requestedRightsToSpawn.Add(metadata.Sender);
                //    return true;
                //}

                //return false;
            }));
        }

        private void OnEntityCreated(int handle)
        {
            if (DoesEntityExist(handle))
            {
                int owner = NetworkGetEntityOwner(handle);
                Player player = PluginManager.PlayersList[owner];

                int populationType = GetEntityPopulationType(handle);

                if (player is not null && populationType == (int)PopulationType.MISSION)
                    queueEntitysCreated.Enqueue(handle);
            }
        }

        [TickHandler]
        private async Task OnEntityQueue()
        {
            if (queueEntitysCreated.Count == 0)
            {
                await BaseScript.Delay(1000);
                return;
            }

            while(queueEntitysCreated.Count > 0)
            {
                int handle = queueEntitysCreated.Peek();

                if (!DoesEntityExist(handle))
                {
                    queueEntitysCreated.Dequeue();
                    continue;
                }

                int owner = NetworkGetEntityOwner(handle);
                Player player = PluginManager.PlayersList[owner];

                if (player is not null)
                {
                    int entityType = GetEntityType(handle);
                    bool isScriptCreated = false;

                    if (entityType == 1)
                    {
                        Ped ped = new Ped(handle);
                        isScriptCreated = ped.State.Get(StateBagKey.CURIOSITY_CREATED) ?? false;
                    }

                    if (entityType == 2)
                    {
                        Vehicle vehicle = new Vehicle(handle);
                        isScriptCreated = vehicle.State.Get(StateBagKey.CURIOSITY_CREATED) ?? false;
                    }

                    if (!isScriptCreated)
                    {
                        DeleteEntity(handle);
                    }
                }
                else
                {
                    queueEntitysCreated.Dequeue();
                }
            }
        }

        private void OnEntityCreating(int handle)
        {
            try
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
                        Player player = PluginManager.PlayersList[owner];

                        if (!requestedRightsToSpawn.Contains(owner))
                        {
                            int entityType = GetEntityType(handle);

                            if (entityType == 1)
                            {
                                Ped ped = new Ped(handle);
                                bool isScriptCreated = ped.State.Get(StateBagKey.PED_MISSION) ?? false;

                                if (isScriptCreated)
                                    goto MoveForwards;
                            }

                            if (entityType == 2)
                            {
                                Vehicle vehicle = new Vehicle(handle);
                                bool isScriptCreated = vehicle.State.Get(StateBagKey.VEHICLE_MISSION) ?? false;

                                if (isScriptCreated)
                                    goto MoveForwards;
                            }

                            DeleteEntity(handle);
                            CancelEvent();
                        }

                    MoveForwards:
                        requestedRightsToSpawn.Remove(owner);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "OnEntityCreating");
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
