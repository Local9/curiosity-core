using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.Classes
{
    [Serializable] // WORK on entity inheritance
    internal class Vehicle : Entity, IEquatable<Vehicle>
    {
        internal CitizenFX.Core.Vehicle Fx { get; private set; }
        public Vector3 Position => Fx.Position;
        public string Hash => Fx.Model.ToString();

        public string Name => API.GetLabelText(API.GetDisplayNameFromVehicleModel((uint)Fx.Model.Hash));

        internal bool IsTowable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_TOW);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_TOW, value);
            }
        }

        internal bool IsSearchable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_SEARCH);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_SEARCH, value);
            }
        }

        internal bool IsMission
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_MISSION);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_MISSION, value);
            }
        }

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

        internal void Impound()
        {

        }
    }
}
