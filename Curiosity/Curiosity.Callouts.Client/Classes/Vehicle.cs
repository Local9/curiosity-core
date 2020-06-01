using CitizenFX.Core;
using Curiosity.Callouts.Client.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Classes
{
    [Serializable]
    internal class Vehicle
    {
        internal CitizenFX.Core.Vehicle Fx { get; private set; }
        public Vector3 Position => Fx.Position;
        public string Name => Fx.Model.ToString();

        public bool IsSpikable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, "curiosity::police::vehicle::mission");
            }
            set
            {
                Decorators.Set(Fx.Handle, "curiosity::police::vehicle::mission", value);
            }
        }

        private Vehicle(CitizenFX.Core.Vehicle fx) => Fx = fx;

        public static async Task<Vehicle> Spawn(Model model, Vector3 position, bool IsSpikable = false)
        {
            Vector3 streetPosition = position.Street();
            CitizenFX.Core.Vehicle fxVehicle = await World.CreateVehicle(model, streetPosition);
            Logger.Log(fxVehicle.ToString());

            return new Vehicle(fxVehicle);
        }

        internal void Dismiss()
        {
            Fx.IsPersistent = false;
        }
    }
}
