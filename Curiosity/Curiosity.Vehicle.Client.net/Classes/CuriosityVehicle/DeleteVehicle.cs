using CitizenFX.Core;
using Curiosity.Global.Shared;
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

        static void OnDeleteNetworkID(int networkId)
        {
            try
            {
                int vehicleId = NetworkGetEntityFromNetworkId(networkId);

                if (vehicleId > 0)
                {

                    if (DoesEntityExist(vehicleId))
                    {

                        FreezeEntityPosition(vehicleId, true);
                        SetEntityAsMissionEntity(vehicleId, false, false);
                        NetworkFadeOutEntity(vehicleId, true, false);
                        SetEntityCoords(vehicleId, -2000f, -6000f, 0f, false, false, false, true);
                        SetEntityAsNoLongerNeeded(ref vehicleId);
                        DeleteEntity(ref vehicleId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDelete -> {ex.Message}");
            }
        }

        static async void OnDelete(string encodedNetworkId)
        {
            try
            {
                if (string.IsNullOrEmpty(encodedNetworkId)) return;

                string decryptedNetworkId = Encode.BytesToStringConverted(System.Convert.FromBase64String(encodedNetworkId));

                int vehicleId = NetworkGetEntityFromNetworkId(int.Parse(decryptedNetworkId));

                if (vehicleId > 0)
                {

                    if (DoesEntityExist(vehicleId))
                    {
                        CitizenFX.Core.Vehicle veh = new CitizenFX.Core.Vehicle(vehicleId);

                        if (veh == null) return;

                        if (!veh.Exists()) return;

                        if (veh.Driver.Handle == Game.PlayerPed.Handle)
                        {
                            if (veh.Handle == Plugin.CurrentVehicle.Handle)
                                Plugin.CurrentVehicle = null;

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDelete -> {ex.Message}");
            }
        }
    }
}
