using CitizenFX.Core;
using Curiosity.MissionManager.Client.Diagnostics;
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

        public static void Start()
        {
            Instance.AttachTickHandler(OnVehicleCreator);
        }

        public static void Stop()
        {
            Instance.DetachTickHandler(OnVehicleCreator);
        }

        private static async Task OnVehicleCreator()
        {
            try
            {
                List<CitizenFX.Core.Vehicle> vehicles = World.GetAllVehicles().Where(x => x.IsInRangeOf(Game.PlayerPed.Position, 30f)).ToList();

                if (vehicles.Count == 0)
                {
                    await BaseScript.Delay(1500);
                }

                vehicles.ForEach(veh =>
                {
                    Vehicle vehicle = new Vehicle(veh, false);
                });
            }
            catch(Exception ex)
            {
                Logger.Error($"OnVehicleCreator -> {ex}");
            }
        }
    }
}
