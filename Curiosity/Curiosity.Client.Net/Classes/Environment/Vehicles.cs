using CitizenFX.Core.Native;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net;

namespace Curiosity.Client.net.Classes.Environment
{
    class Vehicles
    {
        private const string TROUBLE_LICENSE_PLATE = "TROUBLES";
        static Client client = Client.GetInstance();

        public static string STAFF_LICENSE_PLATE = "LV0STAFF";
        public static string HSTAFF_LICENSE_PLATE = "LV0HSTAF";
        public static string DEV_LICENSE_PLATE = "LIFEVDEV";

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

        static async Task InsideVehicleTick()
        {
            await Task.FromResult(0);
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                if (vehicle.Driver.Handle == Game.PlayerPed.Handle)
                {
                    if (
                        vehicle.Mods.LicensePlate == STAFF_LICENSE_PLATE
                        || vehicle.Mods.LicensePlate == HSTAFF_LICENSE_PLATE
                        )
                    {
                        if (!Player.PlayerInformation.IsStaff())
                        {
                            Game.PlayerPed.Task.LeaveVehicle();
                            SentNotification();
                        }
                    }

                    if (vehicle.Mods.LicensePlate == DEV_LICENSE_PLATE)
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER))
                        {
                            Game.PlayerPed.Task.LeaveVehicle();
                            SentNotification();
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
                        vehicle.Mods.LicensePlate.Trim() == STAFF_LICENSE_PLATE
                        || vehicle.Mods.LicensePlate.Trim() == HSTAFF_LICENSE_PLATE
                        )
                    {
                        if (!Player.PlayerInformation.IsStaff())
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                            SentNotification();
                        }

                        if (Player.PlayerInformation.IsStaff())
                        {
                            vehicle.LockStatus = VehicleLockStatus.Unlocked;
                        }
                        return;
                    }

                    if (
                        vehicle.Mods.LicensePlate.Trim() == TROUBLE_LICENSE_PLATE
                        )
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.PROJECTMANAGER))
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                            SentNotification();
                        }

                        if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.PROJECTMANAGER)
                        {
                            vehicle.LockStatus = VehicleLockStatus.Unlocked;
                        }
                        return;
                    }

                    if (vehicle.Mods.LicensePlate.Trim() == DEV_LICENSE_PLATE)
                    {
                        if (!(Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER))
                        {
                            vehicle.LockStatus = VehicleLockStatus.Locked;
                            SentNotification();
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
                    BaseScript.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", networkId);
                }
                else
                {

                    if (Player.PlayerInformation.IsDeveloper())
                        Log.Verbose("Vehicle trying to enter is not the drivers seat");
                }
            }
        }

        static void SentNotification()
        {
            Environment.UI.Notifications.LifeV(1, "Invalid Role", "Sorry you cannot drive this car", string.Empty, 2);
        }
    }
}
