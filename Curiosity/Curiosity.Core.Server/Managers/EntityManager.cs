using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        public static EntityManager EntityInstance;
        const string ENTITY_VEHICLE_DELETE = "delete:vehicle";
        const string ENTITY_VEHICLE_REPAIR = "repair:vehicle";

        public override void Begin()
        {
            EntityInstance = this;

            EventSystem.GetModule().Attach("entity:nuke", new EventCallback(metadata =>
            {
                EventSystem.SendAll("entity:nuke");
                return null;
            }));

            EventSystem.GetModule().Attach("delete:entity", new EventCallback(metadata =>
            {
                int networkId = metadata.Find<int>(0);

                NetworkDeleteEntity(networkId);

                return null;
            }));

            EventSystem.GetModule().Attach("entity:spawn:vehicle", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;
                Player player = PluginManager.PlayersList[metadata.Sender];

                RoutingBucket routingBucket = PluginManager.ActiveUsers[metadata.Sender].RoutingBucket;

                uint model = metadata.Find<uint>(0);
                float x = metadata.Find<float>(1);
                float y = metadata.Find<float>(2);
                float z = metadata.Find<float>(3);
                float h = metadata.Find<float>(4);

                int vehicleId = API.CreateVehicle(model, x, y, z, h, true, true);

                await BaseScript.Delay(0);

                if (vehicleId == 0)
                {
                    Logger.Debug($"Possible OneSync is Disabled");
                    return null;
                }

                DateTime maxWaitTime = DateTime.UtcNow.AddSeconds(5);

                while (!API.DoesEntityExist(vehicleId))
                {
                    await BaseScript.Delay(0);

                    if (maxWaitTime < DateTime.UtcNow) break;
                }

                if (!API.DoesEntityExist(vehicleId))
                {
                    Logger.Debug($"Failed to create vehicle in timely manor.");
                    return null;
                }

                await BaseScript.Delay(0);

                API.SetEntityDistanceCullingRadius(vehicleId, 15000f);
                API.SetEntityRoutingBucket(vehicleId, (int)routingBucket);

                await BaseScript.Delay(0);

                Vehicle veh = new Vehicle(vehicleId);
                veh.State.Set(StateBagKey.VEH_SPAWNED, true, true);
                veh.State.Set(StateBagKey.PLAYER_OWNER, metadata.Sender, true);
                veh.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                veh.State.Set(StateBagKey.VEHICLE_MISSION, true, true);
                veh.State.Set(StateBagKey.VEHICLE_STOLEN, false, true);
                veh.State.Set(StateBagKey.VEHICLE_FLEE, false, true);
                veh.State.Set(StateBagKey.VEHICLE_SEARCH, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TOW, false, true);
                veh.State.Set(StateBagKey.VEHICLE_IMPORTANT, false, true);
                veh.State.Set(StateBagKey.VEHICLE_SETUP, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_HANDLE, 0, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_MARKED, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_PULLOVER, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_IGNORED, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_COMPLETED, false, true);

                return API.NetworkGetNetworkIdFromEntity(vehicleId);
            }));

            EventSystem.GetModule().Attach("entity:spawn:ped", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;
                Player player = PluginManager.PlayersList[metadata.Sender];

                RoutingBucket routingBucket = PluginManager.ActiveUsers[metadata.Sender].RoutingBucket;

                int pedType = metadata.Find<int>(0);
                uint model = metadata.Find<uint>(1);
                float x = metadata.Find<float>(2);
                float y = metadata.Find<float>(3);
                float z = metadata.Find<float>(4);
                float h = metadata.Find<float>(5);

                int pedId = API.CreatePed(pedType, model, x, y, z, h, true, true);
                await BaseScript.Delay(0);

                if (pedId == 0)
                {
                    Logger.Debug($"Possible OneSync is Disabled");
                    return null;
                }

                DateTime maxWaitTime = DateTime.UtcNow.AddSeconds(5);

                while (!API.DoesEntityExist(pedId))
                {
                    await BaseScript.Delay(0);

                    if (maxWaitTime < DateTime.UtcNow) break;
                }

                API.SetEntityRoutingBucket(pedId, (int)routingBucket);
                API.SetEntityDistanceCullingRadius(pedId, 15000f);

                await BaseScript.Delay(0);

                if (!API.DoesEntityExist(pedId))
                {
                    Logger.Debug($"Failed to create ped in timely manor.");
                    return null;
                }

                Ped ped = new Ped(pedId);
                ped.State.Set(StateBagKey.PLAYER_OWNER, metadata.Sender, true);
                ped.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                ped.State.Set(StateBagKey.PED_SPAWNED, true, true);
                ped.State.Set(StateBagKey.PED_FLEE, false, true);
                ped.State.Set(StateBagKey.PED_SHOOT, false, true);
                ped.State.Set(StateBagKey.PED_FRIENDLY, false, true);
                ped.State.Set(StateBagKey.PED_ARREST, false, true);
                ped.State.Set(StateBagKey.PED_ARRESTED, false, true);
                ped.State.Set(StateBagKey.PED_ARRESTABLE, false, true);
                ped.State.Set(StateBagKey.PED_SUSPECT, false, true);
                ped.State.Set(StateBagKey.PED_MISSION, true, true);
                ped.State.Set(StateBagKey.PED_IMPORTANT, false, true);
                ped.State.Set(StateBagKey.PED_HOSTAGE, false, true);
                ped.State.Set(StateBagKey.PED_RELEASED, false, true);
                ped.State.Set(StateBagKey.PED_HANDCUFFED, false, true);
                ped.State.Set(StateBagKey.PED_DIALOGUE, false, true);
                ped.State.Set(StateBagKey.PED_SETUP, false, true);
                ped.State.Set(StateBagKey.PED_IS_DRIVER, false, true);
                // menu options
                ped.State.Set(StateBagKey.MENU_RANDOM_RESPONSE, 0, true);
                ped.State.Set(StateBagKey.MENU_WELCOME, false, true);
                ped.State.Set(StateBagKey.MENU_IDENTIFICATION, false, true);
                ped.State.Set(StateBagKey.MENU_WHAT_YOU_DOING, false, true);
                ped.State.Set(StateBagKey.MENU_RAN_RED_LIGHT, false, true);
                ped.State.Set(StateBagKey.MENU_SPEEDING, false, true);
                ped.State.Set(StateBagKey.MENU_LANE_CHANGE, false, true);
                ped.State.Set(StateBagKey.MENU_TAILGATING, false, true);

                return API.NetworkGetNetworkIdFromEntity(pedId);
            }));

            EventSystem.GetModule().Attach("entity:spawn:prop", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                return await CreateEntity(metadata);
            }));

            EventSystem.GetModule().Attach("entity:damage", new EventCallback(metadata =>
            {
                //BaseScript.TriggerClientEvent("c:mm:damage", networkId, x, y, z, force, radius, fromEntity, numberOfHits);

                int networkId = metadata.Find<int>(0);
                float x = metadata.Find<float>(1);
                float y = metadata.Find<float>(2);
                float z = metadata.Find<float>(3);
                float force = metadata.Find<float>(4);
                float radius = metadata.Find<float>(5);
                int fromEntity = metadata.Find<int>(6);
                int numberOfHits = metadata.Find<int>(7);

                return null;
            }));

            EventSystem.GetModule().Attach(ENTITY_VEHICLE_DELETE, new EventCallback(metadata =>
            {
                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];

                Vector3 position = Vector3.Zero;
                position.X = metadata.Find<float>(0);
                position.Y = metadata.Find<float>(1);
                position.Z = metadata.Find<float>(2);

                bool result = ConfigManager.GetModule().IsNearEventLocation(position, ENTITY_VEHICLE_DELETE);

                Logger.Debug($"{user.LatestName} delete vehicle; {result}");

                if (result)
                    user.Send(ENTITY_VEHICLE_DELETE);

                return null;
            }));

            EventSystem.GetModule().Attach(ENTITY_VEHICLE_REPAIR, new AsyncEventCallback(async metadata =>
            {
                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];

                Vector3 position = Vector3.Zero;
                position.X = metadata.Find<float>(0);
                position.Y = metadata.Find<float>(1);
                position.Z = metadata.Find<float>(2);

                bool result = ConfigManager.GetModule().IsNearEventLocation(position, ENTITY_VEHICLE_REPAIR);

                Logger.Debug($"{user.LatestName} repair vehicle; {result}");

                if (result)
                {
                    if (user.Character.Cash < 100)
                    {
                        return null;
                    }

                    user.Character.Cash = await Database.Store.BankDatabase.Adjust(user.Character.CharacterId, -100);

                    user.Send(ENTITY_VEHICLE_REPAIR);
                }

                return null;
            }));

            Instance.ExportDictionary.Add("CreateProp", new Func<string, uint, float, float, float, bool, Task<string>>(
                async (playerHandle, model, x, y, z, isDynamic) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    int propNetworkId = await CreateEntity(playerId, model, x, y, z, isDynamic);

                    exportMessage.NetworkId = propNetworkId;

                    return $"{exportMessage}";
                }));
        }

        private async Task<object> CreateEntity(EventMetadata metadata)
        {
            uint model = metadata.Find<uint>(0);
            float x = metadata.Find<float>(1);
            float y = metadata.Find<float>(2);
            float z = metadata.Find<float>(3);
            bool isDynamic = metadata.Find<bool>(6);

            return await CreateEntity(metadata.Sender, model, x, y, z, isDynamic);
        }

        private async Task<int> CreateEntity(int source, uint model, float x, float y, float z, bool isDynamic)
        {
            RoutingBucket routingBucket = PluginManager.ActiveUsers[source].RoutingBucket;

            int objectId = API.CreateObjectNoOffset(model, x, y, z, true, true, isDynamic);

            Logger.Debug($"Generated Object with ID {objectId}");

            await BaseScript.Delay(0);

            if (objectId == 0)
            {
                Logger.Debug($"Possible OneSync is Disabled");
                return -1;
            }

            DateTime maxWaitTime = DateTime.UtcNow.AddSeconds(5);

            while (!API.DoesEntityExist(objectId))
            {
                await BaseScript.Delay(0);

                if (maxWaitTime < DateTime.UtcNow) break;
            }

            if (!API.DoesEntityExist(objectId))
            {
                Logger.Debug($"Failed to create object in timely manor.");
                return -1;
            }

            API.SetEntityRoutingBucket(objectId, (int)routingBucket);

            API.SetEntityDistanceCullingRadius(objectId, 15000f);

            return API.NetworkGetNetworkIdFromEntity(objectId);
        }

        public void NetworkDeleteEntity(int networkId)
        {
            int entityId = API.NetworkGetEntityFromNetworkId(networkId);

            if (API.DoesEntityExist(entityId))
                API.DeleteEntity(entityId);
        }
    }
}
