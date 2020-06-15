using CitizenFX.Core;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.EventWrapper;
using Curiosity.Callouts.Shared.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers
{
    [Serializable]
    internal abstract class Callout
    {
        // Consider manual callout end
        static PluginManager PluginInstance => PluginManager.Instance;
        public event Action<bool> Ended;
        public bool IsSetup = false;
        public bool IsRunning = false;

        internal virtual string Name { get; set; }

        protected internal List<Player> Players { get; }
        protected int progress = 1;

        public List<Vehicle> RegisteredVehicles { get; }
        public List<Ped> RegisteredPeds { get; }
        public int NumberArrested { get; set; } = 0;

        protected Callout(Player primaryPlayer)
        {
            RegisteredVehicles = new List<Vehicle>();
            RegisteredPeds = new List<Ped>();
            Players = new List<Player> { primaryPlayer };
            PluginInstance.RegisterTickHandler(OnCalloutTask);
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
            Decorators.Set(player.Character.Handle, Decorators.PLAYER_ASSISTING, true);
        }

        internal virtual void Prepare()
        {
            this.IsSetup = false;
            Logger.Log($"Preparing callout {GetType().Name}");
        }

        internal abstract void Tick();

        internal async Task OnCalloutTask()
        {
            if (!IsSetup) return;

            int numberOfAlivePlayers = Players.Select(x => x).Where(x => x.IsAlive).Count();

            if (numberOfAlivePlayers == 0) // clear callout
            {
                End(true);
            }

            await BaseScript.Delay(100);
        }

        internal virtual void End(bool forcefully = false, CalloutMessage calloutMessage = null)
        {
            PluginInstance.DeregisterTickHandler(OnCalloutTask);

            RegisteredPeds.ForEach(ped => ped?.Dismiss());
            RegisteredVehicles.ForEach(vehicle => vehicle?.Dismiss());

            if (Players.Count > 0)
            {
                // Mark to update the blip script about the players state
                Players.ForEach(player =>
                {
                    Decorators.Set(player.Character.Handle, Decorators.PLAYER_ASSISTING, false);
                });
            }

            if (calloutMessage != null)
            {
                calloutMessage.Success = !forcefully;
                calloutMessage.NumberArrested = this.NumberArrested;

                string jsonMessage = JsonConvert.SerializeObject(calloutMessage);
                string encoded = Encode.StringToBase64(jsonMessage);
                BaseScript.TriggerServerEvent(Events.Server.Callout.CompleteCallout, encoded);
            }

            Ended?.Invoke(forcefully);
        }

        internal void RegisterPed(Ped ped)
        {
            ped.Fx.IsPersistent = true;
            RegisteredPeds.Add(ped);
            Logger.Log($"Registered ped {ped.Name} to callout {GetType().Name}");
        }

        internal void RegisterVehicle(Vehicle vehicle)
        {
            vehicle.Fx.IsPersistent = true;
            RegisteredVehicles.Add(vehicle);
            Logger.Log($"Registered vehicle {vehicle.Hash} to callout {GetType().Name}");
        }
    }
}
