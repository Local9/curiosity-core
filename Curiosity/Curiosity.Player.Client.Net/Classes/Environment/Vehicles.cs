﻿using CitizenFX.Core.Native;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment
{
    class Vehicles
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicles:Remove", new Action<int>(OnVehicleRemove));
            client.RegisterTickHandler(OnVehicleEnterTick);
            client.RegisterTickHandler(InsideVehicleTick);
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
                        if (!vehicle.Driver.IsPlayer)
                        {
                            API.SetVehicleHasBeenOwnedByPlayer(vehicle.Handle, false);
                            await Client.Delay(0);
                            API.SetEntityAsMissionEntity(vehicle.Handle, false, false);
                            await Client.Delay(0);
                            API.NetworkFadeOutEntity(vehicle.Handle, true, false);
                            await Client.Delay(0);
                            int copyRef = vehicle.Handle;

                            if (API.DoesEntityExist(vehicle.Handle))
                            {
                                API.DeleteEntity(ref copyRef);

                                if (!API.DoesEntityExist(vehicle.Handle))
                                {
                                    if (Player.PlayerInformation.IsDeveloper())
                                    {
                                        Debug.WriteLine($"OnVehicleRemove -> Removed vehicle with handle {vehicleHandle}");
                                    }

                                    BaseScript.TriggerServerEvent("curiosity:Server:Vehicles:RemoveFromTempStore", vehicleHandle);
                                }
                                else
                                {
                                    if (Player.PlayerInformation.IsDeveloper())
                                    {
                                        Debug.WriteLine($"OnVehicleRemove -> Failed to remove vehicle with handle {vehicleHandle}");
                                    }
                                }
                            }
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

        static async Task InsideVehicleTick()
        {
            await Task.FromResult(0);
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                if (vehicle.Driver.Handle == Game.PlayerPed.Handle)
                {
                    if (
                        vehicle.Mods.LicensePlate == "LIFE-V-STAFF"
                        || vehicle.Mods.LicensePlate == "LIFE-V-HEAD"
                        )
                    {
                        if (!Player.PlayerInformation.IsStaff())
                        {
                            Game.PlayerPed.Task.LeaveVehicle();
                        }
                        return;
                    }

                    if (vehicle.Mods.LicensePlate == "LIFE-V-DEV")
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER))
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                        }
                        return;
                    }
                }
            }
        }

        static async Task OnVehicleEnterTick()
        {
            await Task.FromResult(0);
            int veh = API.GetVehiclePedIsTryingToEnter(Game.PlayerPed.Handle);
            if (veh != 0)
            {
                CitizenFX.Core.Vehicle vehicle = new CitizenFX.Core.Vehicle(veh);
                if (API.GetSeatPedIsTryingToEnter(Game.PlayerPed.Handle) == -1)
                {
                    if (
                        vehicle.Mods.LicensePlate == "LIFE-V-STAFF"
                        || vehicle.Mods.LicensePlate == "LIFE-V-HEAD"
                        )
                    {
                        if (!Player.PlayerInformation.IsStaff())
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                        }
                        return;
                    }

                    if (vehicle.Mods.LicensePlate == "LIFE-V-DEV")
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER))
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                        }
                        return;
                    }

                    int networkId = API.VehToNet(vehicle.Handle);
                    API.SetNetworkIdExistsOnAllMachines(networkId, true);
                    API.SetNetworkIdCanMigrate(networkId, true);
                    BaseScript.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", networkId);
                }
            }
        }
    }
}
