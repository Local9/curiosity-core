using CitizenFX.Core;
using Curiosity.Callouts.Client.Classes;
using Curiosity.Callouts.Client.Utils;
using System.Linq;
using System.Threading.Tasks;
using Ped = Curiosity.Callouts.Client.Classes.Ped;

namespace Curiosity.Callouts.Client.Managers
{
    public class PursuitManager : BaseScript
    {
        private static Pursuit activePursuit;

        internal static void StartNewPursuit(Callout callout)
        {
            activePursuit = new Pursuit(callout);
            callout.Ended += OnCalloutEnded;
        }

        private static void OnCalloutEnded(bool forcefully)
        {
            activePursuit?.End();
            activePursuit = null;
        }

        [Tick]
        private async Task OnTick()
        {
            if (activePursuit == null) return;

            foreach (Ped ped in activePursuit.Callout.RegisteredPeds)
            {
                if (ped.IsInVehicle != ped.Fx.IsInVehicle())
                {
                    ped.IsInVehicle = ped.Fx.IsInVehicle();
                    RedistributeCops(ped);
                }

                await Delay(100);
            }
        }

        private void RedistributeCops(Ped ped)
        {
            if (activePursuit == null)
            {
                Logger.Log("No active pursuit running");
                return;
            }

            var nearestCops = activePursuit.cops
                .OrderBy(tuple => Vector3.Distance(tuple.Item1.Position, ped.Position))
                .ToArray();

            var alivePeds = activePursuit.Callout.RegisteredPeds
                .Where(registeredPed => !registeredPed.IsDead)
                .ToArray();

            if (alivePeds.Length == 0) return;

            int numRedistributedCops = nearestCops.Length / alivePeds.Length;
            for (var i = 0; i < numRedistributedCops - 1; i++)
            {
                if (ped.IsInVehicle)
                    nearestCops[i].Item1.Task.VehicleChase(ped.Fx);
                else
                    nearestCops[i].Item1.Task.Arrest(ped.Fx);
            }
        }

        internal static async void AddNewCopToPursuit()
        {
            Vector3 position = Game.PlayerPed.Position.AroundStreet(200f, 500f);
            var newCop = await SpawnManager.CreateNewCop(position);
            if (activePursuit != null) activePursuit.cops.Add(newCop);
            else
            {
                Logger.Log("Unable to add new cop. activePursuit was null.");
            }
        }
    }
}
