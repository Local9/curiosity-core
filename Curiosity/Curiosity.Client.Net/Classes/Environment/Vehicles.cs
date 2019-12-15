using CitizenFX.Core.Native;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net;

namespace Curiosity.Client.net.Classes.Environment
{
    class Vehicles
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicles:Remove", new Action<int>(OnVehicleRemove));
        }

        static async void OnVehicleRemove(int vehicleHandle)
        {
            try
            {
                if (API.NetworkIsHost())
                {
                    int handleId = API.NetworkGetEntityFromNetworkId(vehicleHandle);

                    API.NetworkRequestControlOfEntity(handleId);

                    CitizenFX.Core.Vehicle vehicle = new CitizenFX.Core.Vehicle(handleId);

                    if (Player.PlayerInformation.IsDeveloper())
                    {
                        Debug.WriteLine("OnVehicleRemove Starting");
                    }

                    if (vehicle.Exists())
                    {
                        if (vehicle.PreviouslyOwnedByPlayer)
                        {
                            int entity = 0;
                            API.GetVehicleOwner(vehicle.Handle, ref entity);

                            if (!(API.DecorGetInt(vehicle.Handle, "Player_Vehicle") == -1))
                            {
                                int playerServerId = API.DecorGetInt(vehicle.Handle, "Player_Vehicle");

                                if (entity > 0)
                                {
                                    if (Client.players[playerServerId] == null)
                                    {
                                        await DeleteVehicle(vehicle);
                                    }
                                }
                                else
                                {
                                    await DeleteVehicle(vehicle);
                                }
                            }
                        }
                        else if (!vehicle.Driver.IsPlayer)
                        {
                            await DeleteVehicle(vehicle);
                        }
                        else
                        {
                            int networkId = API.VehToNet(Game.PlayerPed.CurrentVehicle.Handle);
                            API.SetNetworkIdExistsOnAllMachines(networkId, true);
                            API.SetNetworkIdCanMigrate(networkId, true);
                            BaseScript.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", networkId);
                        }
                    }
                    else
                    {
                        BaseScript.TriggerServerEvent("curiosity:Server:Vehicles:RemoveFromTempStore", vehicleHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            await Client.Delay(0);
        }

        static async Task DeleteVehicle(Vehicle vehicle)
        {
            if (vehicle.Driver.IsPlayer) return;

            API.SetVehicleHasBeenOwnedByPlayer(vehicle.Handle, false);
            API.SetEntityAsMissionEntity(vehicle.Handle, false, false);
            API.NetworkFadeOutEntity(vehicle.Handle, true, false);
            await Client.Delay(500);
            int copyRef = vehicle.Handle;

            if (API.DoesEntityExist(vehicle.Handle))
            {
                vehicle.Position = new Vector3(-2000f, -6000f, 0f);
                vehicle.IsPersistent = false;
                vehicle.MarkAsNoLongerNeeded();

                API.DeleteEntity(ref copyRef);

                if (!API.DoesEntityExist(vehicle.Handle))
                {
                    if (Player.PlayerInformation.IsDeveloper())
                    {
                        Debug.WriteLine($"OnVehicleRemove -> Removed vehicle with handle {copyRef}");
                    }

                    BaseScript.TriggerServerEvent("curiosity:Server:Vehicles:RemoveFromTempStore", copyRef);
                }
                else
                {
                    if (Player.PlayerInformation.IsDeveloper())
                    {
                        Debug.WriteLine($"OnVehicleRemove -> Failed to remove vehicle with handle {copyRef}");
                    }
                }
            }
            await Task.FromResult(0);
        }

        static void SentNotification()
        {
            Environment.UI.Notifications.LifeV(1, "Invalid Role", "Sorry you cannot drive this car", string.Empty, 2);
        }
    }
}
