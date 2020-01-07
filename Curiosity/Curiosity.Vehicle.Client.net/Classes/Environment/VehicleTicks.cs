using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    class VehicleTicks
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            //client.RegisterTickHandler(OnVehicleEnterTick);
            //client.RegisterTickHandler(InsideVehicleTick);

            client.RegisterTickHandler(OnExitingVehicleTask);
        }

        static async Task InsideVehicleTick()
        {
            await Task.FromResult(0);
            if (Game.PlayerPed.IsInVehicle())
            {
                CitizenFX.Core.Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                if (vehicle.Driver.Handle == Game.PlayerPed.Handle)
                {
                    if (
                        vehicle.Mods.LicensePlate.Trim() == Client.STAFF_LICENSE_PLATE
                        || vehicle.Mods.LicensePlate.Trim() == Client.HSTAFF_LICENSE_PLATE
                        )
                    {
                        if (!Player.PlayerInformation.IsStaff())
                        {
                            Game.PlayerPed.Task.LeaveVehicle();
                        }
                    }

                    if (vehicle.Mods.LicensePlate.Trim() == Client.TROUBLE_LICENSE_PLATE)
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.PROJECTMANAGER))
                        {
                            Game.PlayerPed.Task.LeaveVehicle();
                        }
                    }

                    if (vehicle.Mods.LicensePlate.Trim() == Client.DEV_LICENSE_PLATE)
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER))
                        {
                            Game.PlayerPed.Task.LeaveVehicle();
                        }
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

                if (API.GetSeatPedIsTryingToEnter(Game.PlayerPed.Handle) != (int)VehicleSeat.Driver)
                {

                }
                else if (API.GetSeatPedIsTryingToEnter(Game.PlayerPed.Handle) == (int)VehicleSeat.Driver)
                {
                    if (
                        vehicle.Mods.LicensePlate.Trim() == Client.STAFF_LICENSE_PLATE
                        || vehicle.Mods.LicensePlate.Trim() == Client.HSTAFF_LICENSE_PLATE
                        )
                    {
                        if (!Player.PlayerInformation.IsStaff())
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                        }

                        if (Player.PlayerInformation.IsStaff())
                        {
                            vehicle.LockStatus = VehicleLockStatus.Unlocked;
                        }
                        return;
                    }

                    if (
                        vehicle.Mods.LicensePlate.Trim() == Client.TROUBLE_LICENSE_PLATE
                        )
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.PROJECTMANAGER))
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                        }

                        if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.PROJECTMANAGER)
                        {
                            vehicle.LockStatus = VehicleLockStatus.Unlocked;
                        }
                        return;
                    }

                    if (vehicle.Mods.LicensePlate.Trim() == Client.DEV_LICENSE_PLATE)
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER))
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                            API.ForceLightningFlash();
                        }

                        if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                        {
                            vehicle.LockStatus = VehicleLockStatus.Unlocked;
                        }
                        return;
                    }

                    int networkId = API.VehToNet(vehicle.Handle);
                    API.SetNetworkIdExistsOnAllMachines(networkId, true);
                    API.SetNetworkIdCanMigrate(networkId, true);
                }
            }
        }

        static async Task OnExitingVehicleTask()
        {
            await Task.FromResult(0);
            if (!Game.PlayerPed.IsInVehicle()) return;

            if (Game.IsControlPressed(2, Control.VehicleExit) && Game.PlayerPed.IsAlive && (Game.PlayerPed.CurrentVehicle.Model.IsCar || Game.PlayerPed.CurrentVehicle.Model.IsBike))
            {
                await BaseScript.Delay(150);
                if (Game.PlayerPed.IsInVehicle() && Game.IsControlPressed(2, Control.VehicleExit) && Game.PlayerPed.IsAlive)
                {
                    API.SetVehicleEngineOn(Game.PlayerPed.CurrentVehicle.Handle, true, true, false);
                    Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                }
                else
                {
                    Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.None);
                }
            }
        }
    }
}
