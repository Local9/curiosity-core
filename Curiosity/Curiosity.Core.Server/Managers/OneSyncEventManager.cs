using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using System;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

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
            // Instance.EventRegistry.Add("entityCreating", new Action<int>(OnEntityCreating));
            Instance.EventRegistry.Add("entityCreated", new Action<int>(OnEntityCreated));

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
            try
            {
                if (DoesEntityExist(handle))
                {
                    int owner = NetworkGetEntityOwner(handle);
                    Player player = PluginManager.PlayersList[owner];

                    if (player is null) return;

                    int entityType = GetEntityType(handle);

                    //int entityPopulation = GetEntityPopulationType(handle);
                    //PopulationType population = (PopulationType)entityPopulation;

                    //bool expectedPopulation = Equals(population, PopulationType.MISSION) || Equals(population, PopulationType.PERMANENT);

                    //if (!expectedPopulation) return;

                    Entity entity = null;

                    if (entityType == 1) 
                        entity = new Ped(handle);

                    if (entityType == 2)
                        entity = new Vehicle(handle);

                    if (entityType == 3)
                        entity = new Prop(handle);

                    if (entity is not null) 
                        entity.State.Set(StateBagKey.CURIOSITY_CREATED, player.Name, true);
                }
            }
            catch (Exception ex)
            {

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
}
