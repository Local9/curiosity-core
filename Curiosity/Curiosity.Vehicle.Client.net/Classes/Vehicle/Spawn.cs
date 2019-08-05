using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
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

            model.MarkAsNoLongerNeeded();

            API.NetworkFadeInEntity(veh.Handle, false);

            Game.PlayerPed.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
            veh.LockStatus = VehicleLockStatus.Unlocked;
            veh.NeedsToBeHotwired = false;
            veh.IsEngineRunning = true;

            if (fuelLevel < 5f)
            {
                fuelLevel = 15f;
            }

            Function.Call(Hash._DECOR_SET_FLOAT, veh.Handle, "Vehicle.Fuel", fuelLevel);

            veh.BodyHealth = 1000f;
            veh.EngineHealth = 1000f;
            veh.PetrolTankHealth = 1000f;

            Blip blip = veh.AttachBlip();
            blip.IsShortRange = false;
            blip.Sprite = BlipSprite.PersonalVehicleCar;
            blip.Priority = 100;
            blip.Name = "Personal Vehicle";

            API.SetVehicleHasBeenOwnedByPlayer(veh.Handle, true);

            Client.CurrentVehicle = veh;

            int networkId = API.VehToNet(veh.Handle);
            API.SetNetworkIdExistsOnAllMachines(networkId, true);
            API.SetNetworkIdCanMigrate(networkId, true);

            Client.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", networkId);
            return true;
        }
    }
}
