using CitizenFX.Core;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Entity;
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

            Log.Verbose("Vehicle Manager Init");
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
