using CitizenFX.Core;
using Curiosity.Vehicles.Client.net.Classes.CurPlayer;
using System;
using System.Threading.Tasks;

namespace Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle
{
    class VehicleBlip
    {
        static Plugin client = Plugin.GetInstance();

        public static void Init()
        {
            client.RegisterTickHandler(OnTask);
        }

        static async Task OnTask()
        {
            await Task.FromResult(0);
            try
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    if (Game.PlayerPed.CurrentVehicle.Driver.Handle == Game.PlayerPed.Handle)
                    {
                        if (Game.PlayerPed.CurrentVehicle != null && Plugin.CurrentVehicle != null)
                        {
                            if (Game.PlayerPed.CurrentVehicle.Handle == Plugin.CurrentVehicle.Handle)
                            {
                                if (Plugin.CurrentVehicle.AttachedBlip != null)
                                    if (Plugin.CurrentVehicle.AttachedBlip.Exists())
                                        Plugin.CurrentVehicle.AttachedBlip.Alpha = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (Plugin.CurrentVehicle != null)
                    {
                        if (Plugin.CurrentVehicle.IsAlive)
                        {
                            if (Plugin.CurrentVehicle.AttachedBlip != null)
                                if (Plugin.CurrentVehicle.AttachedBlip.Exists())
                                    Plugin.CurrentVehicle.AttachedBlip.Alpha = 255;
                        }
                        else
                        {
                            if (Plugin.CurrentVehicle.AttachedBlip != null)
                                if (Plugin.CurrentVehicle.AttachedBlip.Exists())
                                    Plugin.CurrentVehicle.AttachedBlip.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (PlayerInformation.IsDeveloper())
                {
                    Debug.WriteLine($"{ex}");
                }
            }
        }
    }
}
