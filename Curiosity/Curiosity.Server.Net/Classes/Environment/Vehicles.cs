using CitizenFX.Core;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Entity;
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

        private async static void OnVehicleShopAction([FromSource] CitizenFX.Core.Player player, int vehicleShopId)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                long characterId = session.User.CharacterId;

                bool isOwned = await Database.DatabaseVehicles.SelectCharacterVehicle(characterId, vehicleShopId);
                await BaseScript.Delay(10);
                VehicleShopItem vehicleShopItem = await Database.DatabaseVehicles.SelectVehicleShopItem(vehicleShopId);
                await BaseScript.Delay(10);
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

                    if (session.User.Wallet < vehicleShopItem.Cost)
                    {
                        player.NotificationCuriosity("Vehicle Shop", "Sorry, not enough cash");
                        return;
                    }

                    await BaseScript.Delay(10);

                    bool purchased = await Database.DatabaseVehicles.InsertCharacterVehicle(characterId, vehicleShopId);

                    await BaseScript.Delay(10);

                    if (purchased)
                    {
                        session.DecreaseWallet(vehicleShopItem.Cost);
                        Database.DatabaseUsersBank.DecreaseCash(session.User.BankId, vehicleShopItem.Cost);
                        await BaseScript.Delay(10);
                        player.TriggerEvent("curiosity:Client:Vehicle:Shop:Update");
                        return;
                    }
                    else
                    {
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
                string encodedJson = Encode.StringToBase64(json);

                player.TriggerEvent("curiosity:Client:Vehicle:DonatorVehicleList", encodedJson);
            }
            catch (Exception ex)
            {

            }
        }

        static async void OnGetVehiclesForShop([FromSource] CitizenFX.Core.Player player)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                List<VehicleShopItem> vehicles = await Database.DatabaseVehicles.GetVehicleShopItems(Server.serverId, session.User.CharacterId);

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
