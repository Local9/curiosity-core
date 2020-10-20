﻿using CitizenFX.Core;
using Curiosity.Shared.Client.net.Helper;
using System.Threading.Tasks;

namespace Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle
{
    static class DisableAirControls
    {
        public static void Init()
        {
            Plugin.GetInstance().RegisterTickHandler(OnTick);
        }

        private static Task OnTick()
        {
            if (Game.PlayerPed.Exists() && Game.PlayerPed.IsInVehicle() && (Game.PlayerPed.CurrentVehicle.Model.IsCar || Game.PlayerPed.CurrentVehicle.Model.IsBike) && (Game.PlayerPed.CurrentVehicle.IsInAir/* || Game.PlayerPed.CurrentVehicle.IsUpsideDown*/) && !(Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Cycles))
            {
                if (ControlHelper.IsControlJustPressed(Control.VehicleExit))
                    Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.BailOut);
                else
                    Game.DisableAllControlsThisFrame(27);
            }
            return Task.FromResult(0);
        }
    }
}
