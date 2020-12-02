﻿using CitizenFX.Core;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Managers
{
    public class WorldVehicleManager : Manager<WorldVehicleManager>
    {
        static PluginManager Instance => PluginManager.Instance;
        public static WorldVehicleManager VehicleManager;
        public override void Begin()
        {
            Logger.Info($"- [WorldVehicleManager] Begin --------------------");

            VehicleManager = this;
        }

        public void Start()
        {
            Instance.AttachTickHandler(OnVehicleCreator);
        }

        public void Stop()
        {
            Instance.DetachTickHandler(OnVehicleCreator);
        }

        private async Task OnVehicleCreator()
        {
            try
            {
                List<CitizenFX.Core.Vehicle> vehicles = World.GetAllVehicles().Where(x => x.IsInRangeOf(Game.PlayerPed.Position, 15f)).ToList();

                if (vehicles.Count == 0)
                {
                    await BaseScript.Delay(1500);
                }

                vehicles.ForEach(async veh =>
                {
                    bool setup = Decorators.GetBoolean(veh.Handle, Decorators.VEHICLE_SETUP);

                    // NativeWrapper.Draw3DText(veh.Position.X, veh.Position.Y, veh.Position.Z, $"H: {veh.Handle}, S: {setup}", 40f, 15f);

                    if (!setup)
                    {
                        Vehicle vehicle = new Vehicle(veh, false);
                        vehicle.IsImportant = false;
                    }

                    await BaseScript.Delay(100);
                });
            }
            catch(Exception ex)
            {
                Logger.Error($"OnVehicleCreator -> {ex}");
            }
        }
    }
}
