using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle
{
    class Spawn
    {
        static Random random = new Random();
        static Client client = Client.GetInstance();

        static public bool IsSpawning = false;
        static int numberOfVehiclesSpawned = 0;

        static public void Init()
        {
            // just to create an instance
        }

        public static async Task<bool> SpawnVehicle(Model model, Vector3 spawnPosition, float heading, bool installSirens = false, bool staffSpawn = false, string numberPlate = "")
        {
            try
            {
                if (IsSpawning)
                {
                    CitizenFX.Core.UI.Screen.ShowNotification("~b~Vehicle Spawn:\n~r~Cooldown Active");
                    return false;
                }

                client.RegisterTickHandler(OnCooldown);

                float fuelLevel = random.Next(60, 100);

                if (Client.CurrentVehicle != null)
                {
                    if (Client.CurrentVehicle.Exists())
                    {
                        if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle == Client.CurrentVehicle)
                        {
                            Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);
                        }

                        fuelLevel = Function.Call<float>(Hash._DECOR_GET_FLOAT, Client.CurrentVehicle.Handle, "Vehicle.Fuel");
                        API.DecorRemove(Client.CurrentVehicle.Handle, "Player_Vehicle");
                        API.DecorRemove(Client.CurrentVehicle.Handle, "Vehicle.SirensInstalled");
                        SendDeletionEvent($"{Client.CurrentVehicle.NetworkId}");
                    }
                }

                await BaseScript.Delay(500);

                CitizenFX.Core.UI.Screen.ShowSubtitle("Trying to spawn requested vehicle, please wait...");

                var veh = await World.CreateVehicle(model, spawnPosition, heading);

                if (veh == null)
                {
                    return false;
                }

                await Client.Delay(0);

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

                if (API.DecorIsRegisteredAsType(Client.PLAYER_VEHICLE, 3))
                {
                    API.DecorSetInt(veh.Handle, Client.PLAYER_VEHICLE, Game.Player.ServerId);
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
                else if (veh.Model.IsPlane)
                {
                    blip.Sprite = BlipSprite.Plane;
                }
                else if (veh.Model.IsHelicopter)
                {
                    blip.Sprite = BlipSprite.Helicopter;
                }
                else
                {
                    blip.Sprite = BlipSprite.PersonalVehicleCar;
                }

                blip.Priority = 100;
                blip.Name = "Personal Vehicle";

                await Client.Delay(0);

                API.SetVehicleHasBeenOwnedByPlayer(veh.Handle, true);

                Client.CurrentVehicle = veh;

                API.SetResourceKvpInt("curiosity:vehicle", veh.Handle);

                Client.TriggerEvent("curiosity:Player:Menu:VehicleId", veh.Handle);

                await Client.Delay(0);

                API.SetVehicleExclusiveDriver(veh.Handle, Game.PlayerPed.Handle);
                API.SetVehicleExclusiveDriver_2(veh.Handle, Game.PlayerPed.Handle, 1);

                veh.LockStatus = VehicleLockStatus.Unlocked;

                await Client.Delay(0);

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

                await Client.Delay(0);

                veh.IsVisible = true;
                veh.IsInvincible = false;

                model.MarkAsNoLongerNeeded();

                numberOfVehiclesSpawned = numberOfVehiclesSpawned + 1;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"SpawnVehicle -> {ex.Message}");
                IsSpawning = false;
                client.DeregisterTickHandler(OnCooldown);
                return false;
            }
        }



        public static async Task<CitizenFX.Core.Vehicle> SpawnVehicleEmpty(Model model, Vector3 spawnPosition, float heading, bool installSirens = false, bool staffSpawn = false, string numberPlate = "")
        {
            try
            {
                if (IsSpawning)
                {
                    CitizenFX.Core.UI.Screen.ShowNotification("~b~Vehicle Spawn:\n~r~Cooldown Active");
                    return default;
                }

                client.RegisterTickHandler(OnCooldown);

                float fuelLevel = random.Next(60, 100);

                if (Client.CurrentVehicle != null)
                {
                    if (Client.CurrentVehicle.Exists())
                    {
                        if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle == Client.CurrentVehicle)
                        {
                            Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);
                        }

                        fuelLevel = Function.Call<float>(Hash._DECOR_GET_FLOAT, Client.CurrentVehicle.Handle, "Vehicle.Fuel");
                        API.DecorRemove(Client.CurrentVehicle.Handle, "Player_Vehicle");
                        API.DecorRemove(Client.CurrentVehicle.Handle, "Vehicle.SirensInstalled");
                        SendDeletionEvent($"{Client.CurrentVehicle.NetworkId}");
                    }
                }

                await BaseScript.Delay(500);

                CitizenFX.Core.UI.Screen.ShowSubtitle("Trying to spawn requested vehicle, please wait...");

                var veh = await World.CreateVehicle(model, spawnPosition, heading);

                if (veh == null)
                {
                    return default;
                }

                await Client.Delay(0);

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

                if (API.DecorIsRegisteredAsType(Client.PLAYER_VEHICLE, 3))
                {
                    API.DecorSetInt(veh.Handle, Client.PLAYER_VEHICLE, Game.Player.ServerId);
                }

                if (API.DecorIsRegisteredAsType("Vehicle.SirensInstalled", 2) && installSirens)
                {
                    API.DecorSetBool(veh.Handle, "Vehicle.SirensInstalled", installSirens);
                    Classes.Environment.ChatCommands.ShowSirenKeys();
                }

                await Client.Delay(0);

                API.NetworkFadeInEntity(veh.Handle, true);

                veh.LockStatus = VehicleLockStatus.Unlocked;
                veh.NeedsToBeHotwired = false;
                veh.IsEngineRunning = true;

                await Client.Delay(0);

                if (fuelLevel < 5f)
                {
                    fuelLevel = 15f;
                }

                API.DecorSetFloat(veh.Handle, "Vehicle.Fuel", fuelLevel);

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
                else if (veh.Model.IsPlane)
                {
                    blip.Sprite = BlipSprite.Plane;
                }
                else if (veh.Model.IsHelicopter)
                {
                    blip.Sprite = BlipSprite.Helicopter;
                }
                else
                {
                    blip.Sprite = BlipSprite.PersonalVehicleCar;
                }

                blip.Priority = 100;
                blip.Name = "Personal Vehicle";

                await Client.Delay(0);

                API.SetVehicleHasBeenOwnedByPlayer(veh.Handle, true);

                Client.CurrentVehicle = veh;

                API.DecorSetFloat(veh.Handle, "Vehicle.Fuel", fuelLevel);

                API.SetResourceKvpInt("curiosity:vehicle", veh.Handle);

                Client.TriggerEvent("curiosity:Player:Menu:VehicleId", veh.Handle);

                await Client.Delay(0);

                API.SetVehicleExclusiveDriver(veh.Handle, Game.PlayerPed.Handle);
                API.SetVehicleExclusiveDriver_2(veh.Handle, Game.PlayerPed.Handle, 1);

                veh.LockStatus = VehicleLockStatus.Unlocked;

                await Client.Delay(0);

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

                await Client.Delay(0);

                veh.IsVisible = true;
                veh.IsInvincible = false;

                model.MarkAsNoLongerNeeded();

                numberOfVehiclesSpawned = numberOfVehiclesSpawned + 1;

                return veh;
            }
            catch (Exception ex)
            {
                Log.Error($"SpawnVehicle -> {ex.Message}");
                IsSpawning = false;
                client.DeregisterTickHandler(OnCooldown);
                return default;
            }
        }

        static async Task OnCooldown()
        {
            IsSpawning = true;
            long gameTime = API.GetGameTimer();

            long timerToWait = 5000 * numberOfVehiclesSpawned;

            if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DONATOR)
            {
                timerToWait = 2500 * numberOfVehiclesSpawned;
            }

            if (Player.PlayerInformation.IsStaff())
            {
                timerToWait = 1000;
            }

            if (timerToWait >= 30000)
            {
                timerToWait = 30000;
            }

            while ((API.GetGameTimer() - gameTime) < timerToWait)
            {
                await Client.Delay(1000);
            }
            CitizenFX.Core.UI.Screen.ShowNotification("~b~Vehicle Spawn:\n~g~Cooldown Ended");
            client.DeregisterTickHandler(OnCooldown);
            IsSpawning = false;
        }

        public static void SendDeletionEvent(string vehicleNetworkId)
        {
            int netId = int.Parse(vehicleNetworkId);
            API.SetNetworkIdCanMigrate(netId, true);
            string encodedString = Encode.StringToBase64(vehicleNetworkId);
            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Player:Vehicle:Delete", encodedString));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);
        }
    }
}
