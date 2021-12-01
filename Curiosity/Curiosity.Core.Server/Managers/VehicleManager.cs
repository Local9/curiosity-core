using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
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

        private LocationsConfigManager config;

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
            config = LocationsConfigManager.GetModule();

            foreach (string trailer in ALLOWED_TRAILERS)
            {
                trailerHashes.Add(API.GetHashKey(trailer));
            }

            EventSystem.Attach("vehicle:log:player", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                
                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];
                user.PersonalVehicle = netId;

                Player player = PluginManager.PlayersList[metadata.Sender];
                player.State.Set($"{StateBagKey.PLAYER_VEHICLE}", user.PersonalVehicle, true);
                
                Logger.Debug($"vehicle:log:player -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));
            
            EventSystem.Attach("vehicle:log:staff", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);

                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];
                user.StaffVehicle = netId;

                Player player = PluginManager.PlayersList[metadata.Sender];
                player.State.Set($"{StateBagKey.PLAYER_VEHICLE}", user.PersonalVehicle, true);

                Logger.Debug($"vehicle:log:player -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            EventSystem.Attach("vehicle:log:player:trailer", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalTrailer = netId;
                Logger.Debug($"vehicle:log:player:trailer -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            EventSystem.Attach("vehicle:log:player:plane", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalPlane = netId;
                Logger.Debug($"vehicle:log:player:plane -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            EventSystem.Attach("vehicle:log:player:boat", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalBoat = netId;
                Logger.Debug($"vehicle:log:player:boat -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            EventSystem.Attach("vehicle:log:player:helicopter", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalHelicopter = netId;
                Logger.Debug($"vehicle:log:player:helicopter -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            //EventSystem.Attach("vehicle:spawn", new EventCallback(metadata =>
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

            EventSystem.Attach("vehicle:refuel:charge", new AsyncEventCallback(async metadata =>
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

            EventSystem.Attach("vehicle:refuel:jerry", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;
                GenericMessage genericMessage = new GenericMessage();

                if (!PluginManager.ActiveUsers.ContainsKey(senderHandle))
                {
                    genericMessage.Message = "Player not found.";
                    return genericMessage;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[senderHandle];

                int amount = metadata.Find<int>(0);

                float cost = amount * 0.05f;

                bool canPay = (curiosityUser.Character.Cash - cost) >= 0;

                if (!canPay)
                {
                    genericMessage.Message = "Cannot afford to refill jerry can.";
                    return genericMessage;
                }

                curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, (int)cost * -1);

                genericMessage.Success = true;
                genericMessage.Cost = (int)cost;

                return genericMessage;
            }));

            EventSystem.Attach("vehicle:owner", new EventCallback(metadata =>
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

            EventSystem.Attach("vehicle:tow", new AsyncEventCallback(async metadata =>
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

                    return CommonErrors.PurchaseSuccessful;
                }

                return CommonErrors.NotEnoughPoliceRep1000;
            }));

            EventSystem.Attach("vehicle:spawn", new AsyncEventCallback(async metadata =>
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

                API.SetEntityRoutingBucket(vehicleId, (int)routingBucket);
                API.SetEntityDistanceCullingRadius(vehicleId, 15000f);

                return API.NetworkGetNetworkIdFromEntity(vehicleId);
            }));

            EventSystem.Attach("vehicle:spawn:position", new AsyncEventCallback(async metadata =>
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

                API.SetEntityRoutingBucket(vehicleId, (int)routingBucket);
                API.SetEntityDistanceCullingRadius(vehicleId, 15000f);

                return API.NetworkGetNetworkIdFromEntity(vehicleId);
            }));

            EventSystem.Attach("vehicle:trailer:spawn:position", new AsyncEventCallback(async metadata =>
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

            EventSystem.Attach("vehicle:repair", new AsyncEventCallback(async metadata =>
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

            EventSystem.Attach("vehicle:spawn:menu", new AsyncEventCallback(async metadata =>
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

            EventSystem.Attach("vehicle:drive:check", new AsyncEventCallback(async metadata =>
            {
                SqlResult sqlResult = new SqlResult();

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                {
                    sqlResult.Message = "User is missing from session";
                    return sqlResult;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                if (curiosityUser.IsStaff)
                {
                    sqlResult.Message = "User is staff, and is ignored";
                    sqlResult.Success = true;
                    return sqlResult;
                }

                Web.DiscordClient discordClient = Web.DiscordClient.GetModule();

                try
                {
                    if (curiosityUser.Purchasing)
                    {
                        sqlResult.Message = "Vehicle check is currently processing";
                        return sqlResult;
                    }

                    int itemId = metadata.Find<int>(0);
                    int characterId = curiosityUser.Character.CharacterId;

                    curiosityUser.Purchasing = true;

                    CuriosityShopItem item = new CuriosityShopItem();

                    goto GetItem;

                GetItem:
                    item = await Database.Store.ShopDatabase.GetItem(itemId, characterId);

                    if (item is null)
                        goto FailedItemCheckCannotDriveOwner;

                    if (item.NumberOwned == 0)
                        goto FailedItemCheckCannotDriveOwner;

                    goto CheckRoles;

                CheckRoles:
                    // Check if item has roles
                    List<RoleRequirement> roleRequirements = await Database.Store.ShopDatabase.GetRoleRequirements(itemId, characterId);

                    if (roleRequirements.Count > 0)
                    {
                        Role role = curiosityUser.Role;

                        foreach (RoleRequirement roleRequirement in roleRequirements)
                        {
                            if (role == (Role)roleRequirement.RoleId)
                            {
                                goto CheckItems;
                            }
                        }

                        goto FailedRoleCheckCannotDrive;
                    }

                    goto CheckItems; // Goto Item check is there are no role requirements to check

                CheckItems:
                    // check if item has item requirements
                    List<ItemRequirement> itemRequirements = await Database.Store.ShopDatabase.GetItemRequirements(itemId, characterId);

                    if (itemRequirements.Count > 0)
                    {
                        int requirements = itemRequirements.Count;
                        int metRequirements = 0;

                        foreach (ItemRequirement itemRequirement in itemRequirements)
                        {
                            if (itemRequirement.RequirementMet)
                                metRequirements++;
                        }

                        if (metRequirements == requirements) goto CheckSkills;

                        goto FailedItemCheckCannotDrive;
                    }

                    goto CheckSkills; // Goto skill check is there are no item requirements to check

                CheckSkills:
                    // check if item has skill requirements
                    List<SkillRequirement> skillRequirements = await Database.Store.ShopDatabase.GetSkillRequirements(itemId, characterId);

                    if (skillRequirements.Count > 0)
                    {
                        int requirements = skillRequirements.Count;
                        int metRequirements = 0;

                        foreach (SkillRequirement skillRequirement in skillRequirements)
                        {
                            if (skillRequirement.RequirementMet)
                                metRequirements++;
                        }

                        if (metRequirements == requirements)
                            goto CanDriveVehicle;

                        goto FailedSkillCheckCannotDrive;
                    }

                    goto CanDriveVehicle;

                CanDriveVehicle:
                    sqlResult.Success = true;
                    goto ReturnResult;

                FailedItemCheckCannotDriveOwner:
                    sqlResult.Message = "Cannot drive what you do not own";
                    goto FailedDiscordMessageOwn;

                FailedRoleCheckCannotDrive:
                    sqlResult.Message = "Role Requirement not met";
                    goto FailedDiscordMessage;

                FailedItemCheckCannotDrive:
                    sqlResult.Message = "Item Requirement not met";
                    goto FailedDiscordMessage;

                FailedSkillCheckCannotDrive:
                    sqlResult.Message = "Skill Requirement not met";
                    goto FailedDiscordMessage;

                FailedDiscordMessageOwn:
                    discordClient.SendDiscordPlayerLogMessage($"Player '{curiosityUser.LatestName}' tried to drive a vehicle they do not own"); // MOVE TO DB LOG
                    await BaseScript.Delay(0);
                    goto ReturnResult;

                FailedDiscordMessage:
                    discordClient.SendDiscordPlayerLogMessage($"Player '{curiosityUser.LatestName}' vehicle check '{sqlResult.Message}'"); // MOVE TO DB LOG
                    await BaseScript.Delay(0);
                    goto ReturnResult;

                ReturnResult:
                    curiosityUser.Purchasing = false;
                    return sqlResult;

                }
                catch (Exception ex)
                {
                    DiscordClient.GetModule().SendDiscordServerEventLogMessage($"[ERROR] vehicle:drive:check\r{ex}");
                    await BaseScript.Delay(0);

                    Logger.Error(ex, "vehicle:drive:check");
                    curiosityUser.Purchasing = false;
                    sqlResult.Message = "Error when trying to check vehicle, if this continues, please open a ticket.";
                    return sqlResult;
                }
            }));
        }
    }
}
