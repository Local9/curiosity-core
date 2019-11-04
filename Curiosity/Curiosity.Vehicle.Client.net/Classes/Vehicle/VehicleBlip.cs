﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Curiosity.Vehicle.Client.net.Classes.Vehicle
{
    class VehicleBlip
    {
        static Client client = Client.GetInstance();

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
                        if (Game.PlayerPed.CurrentVehicle != null && Client.CurrentVehicle != null)
                        {
                            if (Game.PlayerPed.CurrentVehicle.Handle == Client.CurrentVehicle.Handle)
                            {
                                if (Client.CurrentVehicle.AttachedBlip != null)
                                    if (Client.CurrentVehicle.AttachedBlip.Exists())
                                        Client.CurrentVehicle.AttachedBlip.Alpha = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (Client.CurrentVehicle != null)
                    {
                        if (Client.CurrentVehicle.IsAlive)
                        {
                            if (Client.CurrentVehicle.AttachedBlip != null)
                                if (Client.CurrentVehicle.AttachedBlip.Exists())
                                    Client.CurrentVehicle.AttachedBlip.Alpha = 255;
                        }
                        else
                        {
                            if (Client.CurrentVehicle.AttachedBlip != null)
                                if (Client.CurrentVehicle.AttachedBlip.Exists())
                                    Client.CurrentVehicle.AttachedBlip.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Player.PlayerInformation.IsDeveloper())
                {
                    Debug.WriteLine($"{ex}");
                }
            }
        }
    }
}
