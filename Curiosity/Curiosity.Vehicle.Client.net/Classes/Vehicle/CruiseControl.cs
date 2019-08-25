using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Helper;
using CitizenFX.Core;
using CitizenFX.Core.UI;

namespace Curiosity.Vehicle.Client.net.Classes.Vehicle
{
    class CruiseControl
    {
        static Client client = Client.GetInstance();
        static public bool IsCruiseControlDisabled = true;
        static bool IsCruiseControlActive = false;
        static float SpeedToKeep = -1;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Player:Vehicle:CruiseControl", new Action(ToggleCruiseControl));
            client.RegisterTickHandler(OnTick);
            client.RegisterTickHandler(Cruise);
        }

        static void ToggleCruiseControl()
        {
            IsCruiseControlDisabled = !IsCruiseControlDisabled;
        }

        static async Task OnTick()
        {
            if (!IsCruiseControlDisabled && Game.PlayerPed.IsInVehicle() && (Game.PlayerPed.CurrentVehicle.Model.IsCar || Game.PlayerPed.CurrentVehicle.Model.IsBike))
            {
                if ((Game.PlayerPed.CurrentVehicle.Speed * 2.24f) > 40.0f && ControlHelper.IsControlPressed(Control.VehicleAccelerate, false))
                {
                    SpeedToKeep = Game.PlayerPed.CurrentVehicle.Speed;
                    IsCruiseControlActive = true;
                }
            }

            if (IsCruiseControlActive && ControlHelper.IsControlJustPressed(Control.VehicleAccelerate, false))
            {
                SpeedToKeep++;
            }

            if (IsCruiseControlActive && (ControlHelper.IsControlPressed(Control.VehicleBrake, false) || ControlHelper.IsControlPressed(Control.VehicleHandbrake, false)))
            {
                Disable();
            }

            await Task.FromResult(0);
        }

        static async Task Cruise()
        {
            if (IsCruiseControlDisabled)
            {
                IsCruiseControlActive = false;
            }

            if (IsCruiseControlActive && Game.PlayerPed.IsInVehicle() && (Game.PlayerPed.CurrentVehicle.Model.IsCar || Game.PlayerPed.CurrentVehicle.Model.IsBike))
            {
                if (!Game.PlayerPed.CurrentVehicle.IsOnAllWheels)
                {
                    IsCruiseControlActive = false;
                }

                if (!Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                {
                    IsCruiseControlActive = false;
                }

                while (IsCruiseControlActive)
                {
                    if (Game.PlayerPed.CurrentVehicle.Rotation.X > 32.0f)
                    {
                        Disable();
                    }
                    else
                    {
                        if (!Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                        {
                            Disable();
                        }
                        else if (Game.PlayerPed.CurrentVehicle.IsInAir)
                        {
                            Disable();
                        }
                        else if (!Game.PlayerPed.CurrentVehicle.IsOnAllWheels)
                        {
                            Disable();
                        }
                        else if (IsCruiseControlActive && (ControlHelper.IsControlPressed(Control.VehicleBrake, false) || ControlHelper.IsControlPressed(Control.VehicleHandbrake, false)))
                        {
                            Disable();
                        }
                        else if ((Game.PlayerPed.CurrentVehicle.Speed * 2.24f) > 70.0f)
                        {
                            Disable("~r~Disabled, going too fast.");
                        }
                        else if (Game.PlayerPed.CurrentVehicle.HasCollided)
                        {
                            Disable();
                        }
                        else if (Game.PlayerPed.CurrentVehicle.BodyHealth < 250.0f)
                        {
                            Disable("~r~Vehicle too damaged");
                        }
                        else if (Game.PlayerPed.CurrentVehicle.EngineHealth < 250.0f)
                        {
                            Disable("~r~Engine too damaged");
                        }
                        else if (Game.PlayerPed.CurrentVehicle.PetrolTankHealth < 250.0f)
                        {
                            Disable("~r~Petrol Tank too damaged");
                        }
                        else
                        {
                            Game.PlayerPed.CurrentVehicle.Speed = SpeedToKeep;
                            Screen.ShowSubtitle("~b~Cruise Control: ~g~Active");
                        }
                    }

                    if (IsCruiseControlDisabled)
                    {
                        Disable();
                    }
                    await BaseScript.Delay(5);
                }
            }
            else
            {
                IsCruiseControlActive = false;
            }
            await Task.FromResult(0);
        }

        static void Disable(string message = "~r~Deactivated")
        {
            SpeedToKeep = -1;
            IsCruiseControlActive = false;
            Screen.ShowSubtitle($"~b~Cruise Control: ~s~{message}");
        }
    }
}
