using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class VehicleManager : Manager<VehicleManager>
    {
        // Move these to a config file
        const int VEHICLE_REPAIR_CHARGE = 100;
        const int VEHICLE_TOW_REP = 1000;
        const int VEHICLE_TOW_COST = 1000;

        private ConfigManager config;

        // Move these to a config file
        List<string> ALLOWED_TRAILERS = new List<string>()
        {
            "trailersmall2",
            "trailers",
            "docktrailer",
            "raketrailer",
            "baletrailer",
            "trailerlogs",
            "trailersmall",
            "boattrailer",
            "graintrailer",
            "tvtrailer",
            "armytrailer2",
            "trailers3",
            "trailers2",
            "armytrailer",
            "freighttrailer",
            "trailerlarge",
            "trailers4",
        };

        List<int> trailerHashes = new List<int>();

        public override void Begin()
        {
            config = ConfigManager.GetModule();

            foreach (string trailer in ALLOWED_TRAILERS)
            {
                trailerHashes.Add(API.GetHashKey(trailer));
            }

            EventSystem.GetModule().Attach("vehicle:log:player", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                
                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];
                user.PersonalVehicle = netId;

                Player player = PluginManager.PlayersList[metadata.Sender];
                player.State.Set($"{StateBagKey.PLAYER_VEHICLE}", user.PersonalVehicle, true);
                
                Logger.Debug($"vehicle:log:player -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            EventSystem.GetModule().Attach("vehicle:log:player:trailer", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalTrailer = netId;
                Logger.Debug($"vehicle:log:player:trailer -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            EventSystem.GetModule().Attach("vehicle:log:player:plane", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalPlane = netId;
                Logger.Debug($"vehicle:log:player:plane -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            EventSystem.GetModule().Attach("vehicle:log:player:boat", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalBoat = netId;
                Logger.Debug($"vehicle:log:player:boat -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            EventSystem.GetModule().Attach("vehicle:log:player:helicopter", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalHelicopter = netId;
                Logger.Debug($"vehicle:log:player:helicopter -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            //EventSystem.GetModule().Attach("vehicle:spawn", new EventCallback(metadata =>
            //{
            //    if (arguments.Count <= 0) return;
            //    var model = API.GetHashKey(arguments.ElementAt(0));
            //    float x = arguments.ElementAt(1).ToFloat();
            //    float y = arguments.ElementAt(2).ToFloat();
            //    float z = arguments.ElementAt(3).ToFloat();

            //    Vector3 pos = new Vector3(x, y, z);
            //    int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);
            //    return null
            //}));

            EventSystem.GetModule().Attach("vehicle:refuel:charge", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;
                GenericMessage genericMessage = new GenericMessage();

                if (!PluginManager.ActiveUsers.ContainsKey(senderHandle))
                {
                    genericMessage.Message = "Player not found.";
                    return genericMessage;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[senderHandle];

                float vehicleFuel = metadata.Find<float>(0);
                float cost = (100.0f - vehicleFuel) * 1.35f;

                bool canPay = (curiosityUser.Character.Cash - cost) >= 0;

                if (!canPay)
                {
                    genericMessage.Message = "Cannot afford to refuel";
                    return genericMessage;
                }

                curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, (int)cost * -1);

                genericMessage.Success = true;
                genericMessage.Cost = (int)cost;

                return genericMessage;
            }));

            EventSystem.GetModule().Attach("vehicle:owner", new EventCallback(metadata =>
            {
                int senderHandle = metadata.Sender;
                int networkId = metadata.Find<int>(0);

                foreach (KeyValuePair<int, CuriosityUser> kvp in PluginManager.ActiveUsers)
                {
                    CuriosityUser user = kvp.Value;

                    if (user.PersonalVehicle == networkId)
                        return user.LatestName;

                    if (user.PersonalTrailer == networkId)
                        return user.LatestName;
                }

                return null;
            }));

            EventSystem.GetModule().Attach("vehicle:tow", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;
                int networkId = metadata.Find<int>(0);

                CommonErrors cannotTow = CommonErrors.UnknownError;

                foreach (KeyValuePair<int, CuriosityUser> kvp in PluginManager.ActiveUsers)
                {
                    CuriosityUser user = kvp.Value;

                    if (user.PersonalVehicle == networkId
                        || user.PersonalBoat == networkId
                        || user.PersonalTrailer == networkId
                        || user.PersonalPlane == networkId
                        || user.PersonalHelicopter == networkId)
                    {
                        cannotTow = CommonErrors.VehicleIsOwned;
                    }
                }

                if (cannotTow == CommonErrors.VehicleIsOwned)
                    return CommonErrors.VehicleIsOwned;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[senderHandle];

                int rep = await Database.Store.StatDatabase.Get(curiosityUser.Character.CharacterId, Stat.POLICE_REPUATATION);

                if (rep > VEHICLE_TOW_REP)
                {
                    curiosityUser.Character.Cash = await Database.Store.BankDatabase.Get(curiosityUser.Character.CharacterId);

                    if ((curiosityUser.Character.Cash - VEHICLE_TOW_COST) < VEHICLE_TOW_COST)
                        return CommonErrors.PurchaseUnSuccessful;

                    curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, -VEHICLE_TOW_COST);

                    EntityManager.EntityInstance.NetworkDeleteEntity(networkId);
                    return CommonErrors.PurchaseSuccessful;
                }

                return CommonErrors.NotEnoughPoliceRep1000;
            }));

            EventSystem.GetModule().Attach("vehicle:spawn", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;

                if (!PluginManager.ActiveUsers.ContainsKey(senderHandle)) return null;

                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];
                Player player = PluginManager.PlayersList[senderHandle];

                int routingBucket = user.RoutingBucket;

                uint model = metadata.Find<uint>(0);
                float distance = metadata.Find<float>(1);

                Vector3 pos = player.Character.Position;

                if (GarageManager.GetModule().AnyVehicleNearPoint(pos, distance))
                {
                    Logger.Debug($"Current location is blocked by another vehicle");
                    return null;
                }

                int vehicleId = API.CreateVehicle(model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);

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
                    Logger.Debug($"Failed to create vehicle in timely manner.");
                    return null;
                }

                Vehicle vehicle = new Vehicle(vehicleId);
                vehicle.State.Set(StateBagKey.VEH_SPAWNED, true, true);
                vehicle.State.Set(StateBagKey.VEH_PERSONAL, true, true);
                vehicle.State.Set(StateBagKey.VEH_OWNER_ID, player.Handle, true);
                vehicle.State.Set(StateBagKey.VEH_OWNER, player.Name, true);
                vehicle.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                vehicle.State.Set(StateBagKey.VEHICLE_MISSION, false, true);
                vehicle.State.Set(StateBagKey.VEH_FUEL, 0f, true);
                vehicle.State.Set(StateBagKey.VEH_FUEL_MULTIPLIER, 0f, true);
                vehicle.State.Set(StateBagKey.VEH_FUEL_SETUP, false, true);
                vehicle.State.Set(StateBagKey.VEH_CONTENT, new { }, true);

                vehicle.State.Set(StateBagKey.VEH_SIREN_LIGHTS, false, true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_STATE, false, true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, "", true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_BLIP, false, true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_AIRHORN, false, true);

                API.SetEntityRoutingBucket(vehicleId, (int)routingBucket);
                API.SetEntityDistanceCullingRadius(vehicleId, 15000f);

                return API.NetworkGetNetworkIdFromEntity(vehicleId);
            }));

            EventSystem.GetModule().Attach("vehicle:spawn:position", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;

                if (!PluginManager.ActiveUsers.ContainsKey(senderHandle)) return null;

                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];
                Player player = PluginManager.PlayersList[senderHandle];

                int routingBucket = user.RoutingBucket;

                float x = metadata.Find<float>(1);
                float y = metadata.Find<float>(2);
                float z = metadata.Find<float>(3);
                float h = metadata.Find<float>(4);

                uint model = metadata.Find<uint>(0);
                float distance = metadata.Find<float>(5);

                Vector3 pos = new Vector3(x, y, z);

                if (GarageManager.GetModule().AnyVehicleNearPoint(pos, distance))
                {
                    Logger.Debug($"Current location is blocked by another vehicle");
                    return null;
                }

                int vehicleId = API.CreateVehicle(model, x, y, z, h, true, true);

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
                    Logger.Debug($"Failed to create vehicle in timely manner.");
                    return null;
                }

                Vehicle vehicle = new Vehicle(vehicleId);
                vehicle.State.Set(StateBagKey.VEH_SPAWNED, true, true);
                vehicle.State.Set(StateBagKey.VEH_PERSONAL, true, true);
                vehicle.State.Set(StateBagKey.VEH_OWNER_ID, player.Handle, true);
                vehicle.State.Set(StateBagKey.VEH_OWNER, player.Name, true);
                vehicle.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                vehicle.State.Set(StateBagKey.VEHICLE_MISSION, false, true);
                vehicle.State.Set(StateBagKey.VEH_FUEL, 0f, true);
                vehicle.State.Set(StateBagKey.VEH_FUEL_MULTIPLIER, 0f, true);
                vehicle.State.Set(StateBagKey.VEH_FUEL_SETUP, false, true);
                vehicle.State.Set(StateBagKey.VEH_CONTENT, new { }, true);

                vehicle.State.Set(StateBagKey.VEH_SIREN_LIGHTS, false, true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, "", true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_BLIP, false, true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_AIRHORN, false, true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_STATE, false, true);

                API.SetEntityRoutingBucket(vehicleId, (int)routingBucket);
                API.SetEntityDistanceCullingRadius(vehicleId, 15000f);

                return API.NetworkGetNetworkIdFromEntity(vehicleId);
            }));

            EventSystem.GetModule().Attach("vehicle:trailer:spawn:position", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;

                if (!PluginManager.ActiveUsers.ContainsKey(senderHandle)) return null;

                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];
                Player player = PluginManager.PlayersList[senderHandle];

                int routingBucket = user.RoutingBucket;

                var model = API.GetHashKey(metadata.Find<string>(0));

                if (!trailerHashes.Contains(model))
                {
                    
                    return null;
                }

                float x = metadata.Find<float>(1);
                float y = metadata.Find<float>(2);
                float z = metadata.Find<float>(3);
                float h = metadata.Find<float>(4);

                int vehicleId = API.CreateVehicle((uint)model, x, y, z, h, true, true);

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
                    Logger.Debug($"Failed to create trailer in timely manner.");
                    return null;
                }

                Vehicle vehicle = new Vehicle(vehicleId);
                vehicle.State.Set($"{StateBagKey.VEH_SPAWNED}", true, true);
                vehicle.State.Set($"{StateBagKey.VEH_PERSONAL_TRAILER}", true, true);

                API.SetEntityRoutingBucket(vehicleId, (int)routingBucket);

                return API.NetworkGetNetworkIdFromEntity(vehicleId);
            }));

            EventSystem.GetModule().Attach("vehicle:repair", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;
                var player = PluginManager.PlayersList[metadata.Sender];

                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];
                user.Character.Cash = await Database.Store.BankDatabase.Get(user.Character.CharacterId);

                if (user.Character.Cash < VEHICLE_REPAIR_CHARGE)
                {
                    return false;
                }
                else
                {
                    if ((user.Character.Cash - VEHICLE_REPAIR_CHARGE) < VEHICLE_REPAIR_CHARGE)
                        return false;

                    user.Character.Cash = await Database.Store.BankDatabase.Adjust(user.Character.CharacterId, -VEHICLE_REPAIR_CHARGE);
                    return true;
                }
            }));

            EventSystem.GetModule().Attach("vehicle:spawn:menu", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;
                var player = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];

                Vector3 position = Vector3.Zero;
                position.X = metadata.Find<float>(0);
                position.Y = metadata.Find<float>(1);
                position.Z = metadata.Find<float>(2);

                Location location = config.NearestEventLocation(position, "vehicle:spawn:menu", 3f);

                Logger.Debug($"EVENT: vehicle:spawn:menu|{location.SpawnType}");

                user.Send("vehicle:spawn:menu");

                return null;

                // Select a list of vehicle IDs and Names that the user owns

            }));
        }
    }
}
