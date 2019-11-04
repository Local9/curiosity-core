﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Global.Shared.net;

namespace Curiosity.Vehicle.Client.net.Classes.Vehicle
{
    class Spawn
    {
        static Random random = new Random();

        static bool isSpawning = false;

        public static async Task<bool> SpawnVehicle(Model model, Vector3 spawnPosition, float heading, bool installSirens = false, bool staffSpawn = false, string numberPlate = "")
        {
            try
            {
                if (isSpawning) return false;
                isSpawning = true;

                float fuelLevel = random.Next(60, 100);

                if (Client.CurrentVehicle != null)
                {
                    if (Client.CurrentVehicle.Exists())
                    {
                        fuelLevel = Function.Call<float>(Hash._DECOR_GET_FLOAT, Client.CurrentVehicle.Handle, "Vehicle.Fuel");
                        API.DecorRemove(Client.CurrentVehicle.Handle, "Player_Vehicle");
                        API.DecorRemove(Client.CurrentVehicle.Handle, "Vehicle.SirensInstalled");
                        SendDeletionEvent($"{Client.CurrentVehicle.NetworkId}");
                    }
                }

                CitizenFX.Core.UI.Screen.ShowSubtitle("Trying to spawn requested vehicle, please wait...");

                await model.Request(20000);

                var veh = await World.CreateVehicle(model, spawnPosition, heading);

                if (veh == null)
                {
                    return false;
                }

                await Client.Delay(0);

                model.MarkAsNoLongerNeeded();

                API.SetEntityLoadCollisionFlag(veh.Handle, true);

                API.NetworkDoesNetworkIdExist(veh.NetworkId);
                API.SetEntitySomething(veh.NetworkId, true);
                API.SetNetworkIdCanMigrate(veh.NetworkId, true);
                API.SetNetworkIdExistsOnAllMachines(veh.NetworkId, true);
                API.SetNetworkIdSyncToPlayer(veh.NetworkId, Game.Player.Handle, true);
                API.SetVehicleIsStolen(veh.Handle, false);
                await Client.Delay(0);
                // API.SetEntityCollision(veh.Handle, false, false);
                // API.SetEntityProofs(veh.Handle, true, true, true, true, true, true, true, true);
                API.SetVehicleOnGroundProperly(veh.Handle);
                veh.IsPersistent = true;

                await Client.Delay(0);

                if (API.DecorIsRegisteredAsType("Player_Vehicle", 3))
                {
                    API.DecorSetInt(veh.Handle, "Player_Vehicle", Game.Player.ServerId);
                }

                if (API.DecorIsRegisteredAsType("Vehicle.SirensInstalled", 2) && installSirens)
                {
                    API.DecorSetBool(veh.Handle, "Vehicle.SirensInstalled", installSirens);
                    Classes.Environment.ChatCommands.ShowSirenKeys();
                }

                await Client.Delay(0);

                API.NetworkFadeInEntity(veh.Handle, true);

                Game.PlayerPed.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
                veh.LockStatus = VehicleLockStatus.Unlocked;
                veh.NeedsToBeHotwired = false;
                veh.IsEngineRunning = true;

                await Client.Delay(0);

                if (fuelLevel < 5f)
                {
                    fuelLevel = 15f;
                }

                Function.Call(Hash._DECOR_SET_FLOAT, veh.Handle, "Vehicle.Fuel", fuelLevel);

                await Client.Delay(0);

                veh.Health = 1000;
                veh.BodyHealth = 1000f;
                veh.EngineHealth = 1000f;
                veh.PetrolTankHealth = 1000f;

                await Client.Delay(0);

                Blip blip = veh.AttachBlip();
                blip.IsShortRange = false;

                if (veh.Model.IsBike || veh.Model.IsBicycle)
                {
                    blip.Sprite = BlipSprite.PersonalVehicleBike;
                }
                else
                {
                    blip.Sprite = BlipSprite.PersonalVehicleCar;
                }

                blip.Priority = 100;
                blip.Name = "Personal Vehicle";

                API.SetVehicleHasBeenOwnedByPlayer(veh.Handle, true);

                Client.CurrentVehicle = veh;

                API.SetResourceKvpInt("curiosity:vehicle", veh.Handle);

                Client.TriggerEvent("curiosity:Player:Menu:VehicleId", veh.Handle);

                API.SetVehicleExclusiveDriver(veh.Handle, Game.PlayerPed.Handle);
                API.SetVehicleExclusiveDriver_2(veh.Handle, Game.PlayerPed.Handle, 1);

                Client.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", veh.NetworkId);

                if (staffSpawn)
                {
                    List<int> listOfExtras = new List<int>();

                    for (int i = 0; i < 255; i++)
                    {
                        if (veh.ExtraExists(i))
                            listOfExtras.Add(i);
                    }

                    Client.TriggerEvent("", 1, "Curiosity", "Vehicle Spawned", $"Available Mods can be found in the Debug Console", 2);
                    Debug.WriteLine($"Vehicle Mods: {string.Join(", ", veh.Mods.GetAllMods().Select(m => Enum.GetName(typeof(VehicleModType), m.ModType)))}");
                    if (listOfExtras.Count > 0)
                    {
                        Debug.WriteLine($"Vehicle Extras: '/mod extra number true/false' avaiable: {string.Join(", ", listOfExtras)}");
                    }
                    Client.TriggerEvent("", 1, "Curiosity", "Vehicle Spawned", $"~b~Engine: ~y~MAX~n~~b~Brakes: ~y~MAX~n~~b~Transmission: ~y~MAX", 2);

                    veh.Mods.LicensePlate = numberPlate;
                    veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlack;
                }

                isSpawning = false;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"SpawnVehicle -> {ex.Message}");
                isSpawning = false;
                return false;
            }
        }

        static void SendDeletionEvent(string vehicleNetworkId)
        {
            string encodedString = Encode.StringToBase64(vehicleNetworkId);
            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Player:Vehicle:Delete", encodedString));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);
        }
    }
}
