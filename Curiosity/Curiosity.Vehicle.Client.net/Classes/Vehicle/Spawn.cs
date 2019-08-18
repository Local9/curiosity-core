using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Vehicle.Client.net.Classes.Vehicle
{
    class Spawn
    {
        static Random random = new Random();

        public static async Task<bool> SpawnVehicle(Model model, Vector3 spawnPosition, float heading)
        {
            float fuelLevel = random.Next(60, 100);

            if (Client.CurrentVehicle != null)
            {
                if (Client.CurrentVehicle.Exists())
                {
                    fuelLevel = Function.Call<float>(Hash._DECOR_GET_FLOAT, Client.CurrentVehicle.Handle, "Vehicle.Fuel");

                    if (Client.CurrentVehicle.AttachedBlip.Exists())
                    {
                        Client.CurrentVehicle.AttachedBlip.Delete();
                    }

                    API.NetworkFadeOutEntity(Client.CurrentVehicle.Handle, true, false);
                    Client.CurrentVehicle.IsEngineRunning = false;
                    Client.CurrentVehicle.MarkAsNoLongerNeeded();
                    await Client.Delay(1000);
                    int handle = Client.CurrentVehicle.Handle;
                    API.DeleteEntity(ref handle);
                }
            }

            await model.Request(10000);

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
            API.SetEntityProofs(veh.Handle, true, true, true, true, true, true, true, true);
            API.SetVehicleOnGroundProperly(veh.Handle);
            veh.IsPersistent = true;

            await Client.Delay(0);

            if (API.DecorIsRegisteredAsType("Player_Vehicle", 3))
            {
                API.DecorSetInt(veh.Handle, "Player_Vehicle", Game.Player.ServerId);
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
            blip.Sprite = BlipSprite.PersonalVehicleCar;
            blip.Priority = 100;
            blip.Name = "Personal Vehicle";

            API.SetVehicleHasBeenOwnedByPlayer(veh.Handle, true);

            Client.CurrentVehicle = veh;

            Client.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", veh.NetworkId);

            return true;
        }
    }
}
