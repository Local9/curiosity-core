using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Client.net.Classes.PlayerClasses;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment
{
    class Vehicles
    {
        static Client client = Client.GetInstance();
        static internal List<Marker> MarkersAll = new List<Marker>();
        static internal List<Marker> MarkersClose = new List<Marker>();
        static float contextAoe = 3f; // How close you need to be to see instruction

        static Vector3 scale = new Vector3(5f, 5f, 2f);
        static System.Drawing.Color color = System.Drawing.Color.FromArgb(135, 206, 235);
        static MarkerType markerType = MarkerType.VerticalCylinder;
        static bool playedSound = false;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicles:Remove", new Action<int>(OnVehicleRemove));
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
        }
        private static void OnPlayerSpawned(dynamic obj)
        {
            if (MarkersAll.Count > 0) return;

            MarkersAll.Add(new Marker(new Vector3(1870.172f, 3693.716f, 33.6004f), markerType, color, scale));
            MarkersAll.Add(new Marker(new Vector3(138.3844f, 6634.493f, 31.64594f), markerType, color, scale));

            client.RegisterTickHandler(OnClientVehicleRepair);
            client.RegisterTickHandler(OnClientVehicleRepairMarker);
            client.RegisterTickHandler(OnClientVehicleRepairMarkerUpdate);
        }

        static public async Task OnClientVehicleRepairMarkerUpdate()
        {
            MarkersClose = MarkersAll.ToList().Select(m => m).Where(m => m.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(m.DrawThreshold, 2)).ToList();
            await BaseScript.Delay(1000);
        }

        static public Task OnClientVehicleRepairMarker()
        {
            try
            {
                MarkersClose.ForEach(m =>
                {
                    World.DrawMarker(m.Type, m.Position, m.Direction, m.Rotation, m.Scale, m.Color, false, false, true);
                    NativeWrappers.Draw3DText(m.Position.X, m.Position.Y, m.Position.Z + 1, $"~s~Vehicle Repair $100", 50f, 10f);
                });
            }
            catch (Exception ex)
            {
                // qqq
            }
            return Task.FromResult(0);
        }

        static Marker GetActiveMarker()
        {
            try
            {
                Marker closestMarker = MarkersClose.Where(w => w.Position.DistanceToSquared(Game.PlayerPed.Position) < contextAoe).FirstOrDefault();
                if (closestMarker == null) return null;
                return closestMarker;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static async Task OnClientVehicleRepair()
        {
            if (Game.PlayerPed.CurrentVehicle == null) return;

            Marker m = GetActiveMarker();
            if (m != null)
            {
                if (!playedSound)
                {
                    API.PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
                    playedSound = !playedSound;
                }
                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to Repair Vehicle for $100.");

                if (Game.IsControlJustPressed(0, Control.Pickup) ||
                    Game.IsControlJustReleased(0, Control.Pickup))
                {
                    if (PlayerInformation.playerInfo.Wallet < 100)
                    {
                        Screen.ShowNotification($"~r~Cannot repair~s~, lacking the funds.", true);
                        API.PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

                        await Client.Delay(5000);
                        return;
                    }

                    Game.PlayerPed.CurrentVehicle.Repair();
                    Game.PlayerPed.CurrentVehicle.ClearLastWeaponDamage();
                    Game.PlayerPed.CurrentVehicle.Wash();

                    Client.TriggerServerEvent("curiosity:server:repair:vehicle");

                    API.PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
                    Screen.DisplayHelpTextThisFrame($"Vehicle Repaired.");

                    await Client.Delay(5000);
                }
            }
            else
            {
                if (playedSound) playedSound = !playedSound;
            }
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

                    if (PlayerInformation.IsDeveloper())
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
                        }
                    }
                    else
                    {
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
                    if (PlayerInformation.IsDeveloper())
                    {
                        Debug.WriteLine($"OnVehicleRemove -> Removed vehicle with handle {copyRef}");
                    }
                }
                else
                {
                    if (PlayerInformation.IsDeveloper())
                    {
                        Debug.WriteLine($"OnVehicleRemove -> Failed to remove vehicle with handle {copyRef}");
                    }
                }
            }
            await Task.FromResult(0);
        }
    }
}
