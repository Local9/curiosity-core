using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Classes
{
    [Serializable]
    internal class Ped
    {
        internal CitizenFX.Core.Ped Fx { get; set; }

        public Vector3 Position => Fx.Position;
        internal Tasks Task => Fx.Task;
        internal bool IsDead => Fx.IsDead;
        internal string Name => Fx.Model.ToString();
        internal bool IsInVehicle { get; set; }

        internal Ped(CitizenFX.Core.Ped fx)
        {
            Fx = fx;
        }

        internal static async Task<Ped> Spawn(Model model, Vector3 position)
        {
            Vector3 sidewalkPosition = position.Sidewalk();
            CitizenFX.Core.Ped fxPed = await World.CreatePed(model, sidewalkPosition);
            Logger.Log(fxPed.ToString());
            var ped = new Ped(fxPed);
            return ped;
        }

        internal void PutInVehicle(Vehicle vehicle, VehicleSeat seat = VehicleSeat.Driver) =>
            Fx.SetIntoVehicle(vehicle.Fx, seat);

        internal Blip AttachBlip(BlipColor color, bool showRoute = false)
        {
            Blip blip = Fx.AttachBlip();
            blip.Color = color;
            blip.ShowRoute = showRoute;

            return blip;
        }

        internal void Dismiss()
        {
            foreach (Blip blip in Fx.AttachedBlips) blip.Delete();
            Fx.IsPersistent = false;
        }
    }
}
