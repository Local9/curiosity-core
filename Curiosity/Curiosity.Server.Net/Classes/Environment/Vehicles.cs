using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.Global.Shared.Utils;
using Curiosity.Server.net.Extensions;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;

namespace Curiosity.Server.net.Classes.Environment
{
    class Vehicles
    {
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Vehicle:Detonate", new Action<int>(OnDetonateVehicle));

            server.RegisterEventHandler("curiosity:Server:Vehicle:GetVehicleSpawnLocations", new Action<CitizenFX.Core.Player>(OnGetVehicleSpawnLocations));
            server.RegisterEventHandler("curiosity:Server:Vehicle:GetVehicleList", new Action<CitizenFX.Core.Player, int>(OnGetVehicleList));
            server.RegisterEventHandler("curiosity:Server:Vehicle:GetDonatorVehicleList", new Action<CitizenFX.Core.Player>(OnGetDonatorVehicleList));
            server.RegisterEventHandler("curiosity:Server:Vehicle:Shop:Get", new Action<CitizenFX.Core.Player>(OnGetVehiclesForShop));
            server.RegisterEventHandler("curiosity:Server:Vehicle:Shop:Action", new Action<CitizenFX.Core.Player, int>(OnVehicleShopAction));

            Log.Verbose("Vehicle Manager Init");
        }

        // check if vehicle is owned
        // if not owned then purchase
        // on purchase success, spawn
        // if owned, spawn

        private static float Discount(Privilege privilege)
        {
            float discount = float.Parse(API.GetConvar("shop_event_discount", "0.0"));

            if (discount > 0)
                return discount;

            switch (privilege)
            {
                case Privilege.DONATOR:
                case Privilege.ADMINISTRATOR:
                case Privilege.COMMUNITYMANAGER:
                case Privilege.DEVELOPER:
                case Privilege.HEADADMIN:
                case Privilege.MODERATOR:
                case Privilege.PROJECTMANAGER:
                case Privilege.SENIORADMIN:
                    return float.Parse(API.GetConvar("shop_donator_discount_life", "1.0"));
                case Privilege.DONATOR1:
                    return float.Parse(API.GetConvar("shop_donator_discount_tier1", "1.0"));
                case Privilege.DONATOR2:
                    return float.Parse(API.GetConvar("shop_donator_discount_tier2", "1.0"));
                case Privilege.DONATOR3:
                    return float.Parse(API.GetConvar("shop_donator_discount_tier3", "1.0"));
                default:
                    return 1.0f;
            }
        }

        private async static void OnVehicleShopAction([FromSource] CitizenFX.Core.Player player, int vehicleShopId)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                long characterId = session.User.CharacterId;

                bool isOwned = await Database.DatabaseVehicles.SelectCharacterVehicle(characterId, vehicleShopId);
                await BaseScript.Delay(100);
                VehicleShopItem vehicleShopItem = await Database.DatabaseVehicles.SelectVehicleShopItem(vehicleShopId);
                await BaseScript.Delay(100);
                if (!isOwned)
                {
                    if (vehicleShopItem == null)
                    {
                        player.NotificationCuriosity("Vehicle Shop", "Sorry, try again later.");
                        return;
                    }

                    if (vehicleShopItem.NumberRemaining != null && vehicleShopItem.NumberRemaining == 0)
                    {
                        player.NotificationCuriosity("Vehicle Shop", "Sorry, no more are remaining");
                        return;
                    }

                    int cost = vehicleShopItem.Cost;

                    float costDiscount = Discount(session.Privilege);                    

                    int finalCost = (int)(cost * costDiscount);

                    if (session.User.Wallet < finalCost)
                    {
                        player.NotificationCuriosity("Vehicle Shop", "Sorry, not enough cash");
                        return;
                    }

                    await BaseScript.Delay(100);

                    bool purchased = await Database.DatabaseVehicles.InsertCharacterVehicle(characterId, vehicleShopId);

                    await BaseScript.Delay(100);

                    if (purchased)
                    {
                        session.DecreaseWallet(finalCost);
                        Database.DatabaseUsersBank.DecreaseCash(session.User.BankId, finalCost);
                        await BaseScript.Delay(100);
                        player.TriggerEvent("curiosity:Client:Vehicle:Shop:Update");
                        return;
                    }
                    else
                    {
                        await BaseScript.Delay(100);
                        await Database.DatabaseVehicles.DeleteCharacterVehicle(characterId, vehicleShopId);
                        player.NotificationCuriosity("Vehicle Shop", "Sorry, please try again later");
                        return;
                    }
                }
                else
                {
                    player.TriggerEvent("curiosity:Client:Vehicle:Create", vehicleShopItem.VehicleHash);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnVehicleShopAction");
            }
        }

        static async void OnGetVehicleList([FromSource]CitizenFX.Core.Player player, int spawnId)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                List<VehicleItem> vehicles = await Database.DatabaseVehicles.GetVehiclesForSpawn(spawnId);

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(vehicles);
                string encodedJson = Encode.StringToBase64(json);

                player.TriggerEvent("curiosity:Client:Vehicle:VehicleList", encodedJson);
            }
            catch (Exception ex)
            {

            }
        }

        static async void OnGetDonatorVehicleList([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                List<VehicleItem> vehicles = await Database.DatabaseVehicles.GetVehiclesForDonators(session.UserID);

                if (vehicles != null)
                {
                    if (vehicles.Count == 1)
                    {
                        if (vehicles[0].VehicleHashString == "notdonator")
                        {
                            player.NotificationCuriosity("System Message", "Sorry, this is a donator service");
                            return;
                        }
                    }
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(vehicles);

                player.TriggerLatentEvent("curiosity:Client:Vehicle:DonatorVehicleList", 750000, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[VEHICLE] DONOR LIST");
            }
        }

        static async void OnGetVehiclesForShop([FromSource] CitizenFX.Core.Player player)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                List<VehicleShopItem> vehicles = await Database.DatabaseVehicles.GetVehicleShopItems(Server.serverId, session.User.CharacterId);

                foreach (VehicleShopItem vehicleItem in vehicles)
                {
                    int cost = vehicleItem.Cost;

                    float costDiscount = Discount(session.Privilege);

                    int finalCost = (int)(cost * costDiscount);

                    vehicleItem.Cost = finalCost;
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(vehicles);

                string encodedJson = Encode.StringToBase64(json);

                player.TriggerEvent("curiosity:Client:Vehicle:Shop:Items", encodedJson);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnGetVehiclesForShop");
            }
        }

        static async void OnGetVehicleSpawnLocations([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                List<VehicleSpawnLocation> loc = await Database.DatabaseVehicles.GetVehicleSpawns(Server.serverId);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(loc);
                string encodedJson = Encode.StringToBase64(json);

                player.TriggerEvent("curiosity:Client:Vehicle:SpawnLocations", encodedJson);
            }
            catch (Exception ex)
            {

            }
        }

        static void OnDetonateVehicle(int vehId)
        {
            BaseScript.TriggerClientEvent("curiosity:Client:Vehicle:Detonate", vehId);
        }
    }
}
