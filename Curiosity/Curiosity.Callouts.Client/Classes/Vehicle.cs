using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Callouts.Client.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Classes
{
    [Serializable] // WORK on entity inheritance
    internal class Vehicle : Entity, IEquatable<Vehicle>
    {
        internal CitizenFX.Core.Vehicle Fx { get; private set; }
        public Vector3 Position => Fx.Position;
        public string Name => Fx.Model.ToString();

        public bool IsSpikable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_SPIKE_ALLOWED);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_SPIKE_ALLOWED, value);
            }
        }

        private Vehicle(CitizenFX.Core.Vehicle fx) : base(fx.Handle)
        {
            Fx = fx;
        }

        public static async Task<Vehicle> Spawn(Model model, Vector3 position)
        {
            Vector3 streetPosition = position.Street();
            CitizenFX.Core.Vehicle fxVehicle = await World.CreateVehicle(model, streetPosition);
            Logger.Log(fxVehicle.ToString());

            API.NetworkFadeInEntity(fxVehicle.Handle, false);

            return new Vehicle(fxVehicle);
        }

        internal async void Dismiss()
        {
            Fx.IsPersistent = false;

            API.NetworkFadeOutEntity(base.Handle, false, false);
            await BaseScript.Delay(2000);

            base.Delete();
        }

        protected bool Equals(Vehicle other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Fx, other.Fx));
        }

        public override bool Equals(object obj)
        {
            bool flag;
            if (obj == null)
            {
                flag = false;
            }
            else
            {
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((Vehicle)obj) : true;
            }
            return flag;
        }

        bool IEquatable<Vehicle>.Equals(Vehicle other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Fx, other.Fx));
        }
    }
}
