using CitizenFX.Core;
using Curiosity.Global.Shared;
using Curiosity.Vehicles.Client.net.Classes.CurPlayer;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle
{
    class DeleteVehicle
    {
        static Plugin client = Plugin.GetInstance();

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Player:Vehicle:Delete", new Action<string>(OnDelete));
            client.RegisterEventHandler("curiosity:Player:Vehicle:Delete:NetworkId", new Action<int>(OnDeleteNetworkID));
        }

        async static void OnDeleteNetworkID(int networkId)
        {
            try
            {
                if (PlayerInformation.IsDeveloper())
                    Debug.WriteLine($"OnDeleteNetworkID -> {networkId}");

                int vehicleId = NetworkGetEntityFromNetworkId(networkId);

                if (vehicleId > 0)
                {
                    if (DoesEntityExist(vehicleId))
                    {
                        if (PlayerInformation.IsDeveloper())
                            Debug.WriteLine($"DoesEntityExist -> true");

                        CitizenFX.Core.Vehicle veh = new CitizenFX.Core.Vehicle(vehicleId);

                        if (veh == null) return;

                        if (!veh.Exists()) return;


                        if (veh.Handle == Plugin.CurrentVehicle.Handle)
                            Plugin.CurrentVehicle = null;

                        foreach (Ped p in veh.Passengers)
                        {
                            p.Task.WarpOutOfVehicle(veh);
                            await BaseScript.Delay(500);
                        }

                        Game.PlayerPed.Task.WarpOutOfVehicle(veh);

                        await BaseScript.Delay(2000);

                        NetworkFadeOutEntity(vehicleId, false, false);
                        await BaseScript.Delay(500);
                        veh.Position = new Vector3(-2000f, -6000f, 0f);
                        await BaseScript.Delay(500);
                        veh.MarkAsNoLongerNeeded();
                        veh.Delete();

                        if (PlayerInformation.IsDeveloper())
                            Debug.WriteLine($"Deleted vehicle");

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDeleteNetworkID -> {ex.Message}");
            }
        }

        static async void OnDelete(string encodedNetworkId)
        {
            try
            {
                if (string.IsNullOrEmpty(encodedNetworkId)) return;

                int vehicleId = NetworkGetEntityFromNetworkId(int.Parse(encodedNetworkId));

                if (vehicleId > 0)
                {

                    if (DoesEntityExist(vehicleId))
                    {
                        CitizenFX.Core.Vehicle veh = new CitizenFX.Core.Vehicle(vehicleId);

                        if (veh == null) return;

                        if (!veh.Exists()) return;

                        if (veh.Handle == Plugin.CurrentVehicle.Handle)
                            Plugin.CurrentVehicle = null;

                        foreach (Ped p in veh.Passengers)
                        {
                            p.Task.WarpOutOfVehicle(veh);
                            await BaseScript.Delay(500);
                        }

                        Game.PlayerPed.Task.WarpOutOfVehicle(veh);

                        await BaseScript.Delay(2000);

                        NetworkFadeOutEntity(vehicleId, false, false);
                        await BaseScript.Delay(500);
                        veh.Position = new Vector3(-2000f, -6000f, 0f);
                        await BaseScript.Delay(500);
                        veh.MarkAsNoLongerNeeded();
                        veh.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDelete -> {encodedNetworkId} / {ex.Message}");
            }
        }
    }
}
