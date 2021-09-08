using CitizenFX.Core;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Managers
{
    public class WorldVehicleManager : Manager<WorldVehicleManager>
    {
        ConcurrentDictionary<int, Vehicle> WorldVehicles = new ConcurrentDictionary<int, Vehicle>();

        static PluginManager Instance => PluginManager.Instance;
        public static WorldVehicleManager VehicleManager;
        public override void Begin()
        {
            Logger.Info($"- [WorldVehicleManager] Begin --------------------");

            VehicleManager = this;
        }

        public void Start()
        {
            Logger.Debug($"Traffic Stop System Started");
            Instance.AttachTickHandler(OnVehicleCreator);
            Instance.AttachTickHandler(OnWorldVehicleList);
        }

        public void Stop()
        {
            Logger.Debug($"Traffic Stop System Stopped");
            Instance.DetachTickHandler(OnVehicleCreator);
            Instance.DetachTickHandler(OnWorldVehicleList);

            ConcurrentDictionary<int, Vehicle> WorldVehiclesCopy = WorldVehicles;
            foreach (KeyValuePair<int, Vehicle> kvp in WorldVehiclesCopy)
            {
                Vehicle veh = kvp.Value;

                if (veh.DateCreated.Subtract(DateTime.Now).TotalSeconds > 60 && !veh.IsMission)
                {
                    veh.Dismiss();
                    WorldVehicles.TryRemove(kvp.Key, out Vehicle old);
                }
            }

            WorldVehicles.Clear();
        }

        private async Task OnWorldVehicleList() // WHAT the fuck is this?
        {
            ConcurrentDictionary<int, Vehicle> WorldVehiclesCopy = WorldVehicles;

            foreach (KeyValuePair<int, Vehicle> kvp in WorldVehiclesCopy)
            {
                Vehicle veh = kvp.Value;

                if (!veh.Exists())
                {
                    WorldVehicles.TryRemove(kvp.Key, out Vehicle old);
                }
                else if (veh.DateCreated.Subtract(DateTime.Now).TotalSeconds > 60 && !veh.IsMission)
                {
                    veh.Dismiss();
                    WorldVehicles.TryRemove(kvp.Key, out Vehicle old);
                }
            }

            DateTime startPolling = DateTime.Now;
            while (startPolling.Subtract(DateTime.Now).TotalSeconds < 10)
            {
                await BaseScript.Delay(1000);
            }
        }

        private async Task OnVehicleCreator()
        {
            try
            {
                List<CitizenFX.Core.Vehicle> vehicles = World.GetAllVehicles().Where(x => x.IsInRangeOf(Game.PlayerPed.Position, 30f)).ToList();

                if (vehicles.Count == 0)
                {
                    await BaseScript.Delay(1500);
                }

                vehicles.ForEach(async veh =>
                {
                    bool setup = veh.State.Get(StateBagKey.VEHICLE_SETUP) ?? false || Decorators.GetBoolean(veh.Handle, StateBagKey.VEHICLE_SETUP);

                    if (!setup && !veh.Driver.IsPlayer)
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
