using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Classes
{
    [Serializable] // WORK on entity inheritance
    public class Vehicle : Entity, IEquatable<Vehicle>
    {
        public CitizenFX.Core.Vehicle Fx { get; private set; }
        public Vector3 Position => Fx.Position;
        public string Hash => Fx.Model.ToString();

        public string Name => API.GetLabelText(API.GetDisplayNameFromVehicleModel((uint)Fx.Model.Hash));

        public bool IsTowable
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

        public bool IsSearchable
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

        public bool IsMission
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
            
            API.ClearAreaOfEverything(streetPosition.X, streetPosition.Y, streetPosition.Z, 5f, false, false, false, false);

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
            // ImpoundManager.Tow(this);
        }

        public void BurstWheel(Wheels wheel, bool onRim = false, float dmg = 1000f)
        {
            API.SetVehicleTyreBurst(Fx.Handle, (int)wheel, onRim, dmg);
        }

        public void DamageTop(float force = 1000f, float radius = 1000f, int numberOfHits = 1)
        {
            for (var i = 0; i < numberOfHits; i++)
                API.SetVehicleDamage(Fx.Handle, 0f, 1f, 1f, force, radius, true);
        }

        public void DamageFront(bool increaseDamage = false)
        {

            API.SetVehicleDamage(Fx.Handle, 0f, 1.2f, 0f, 1600f, 1600f, true);
            API.SetVehicleDamage(Fx.Handle, 0f, 0.75f, 0.05f, 1600f, 1600f, true);
            API.SetVehicleDamage(Fx.Handle, -0.7f, 0f, 0f, 1600f, 1600f, true);
            API.SetVehicleDamage(Fx.Handle, 0.7f, 0f, 0f, 1600f, 1600f, true);

            if (increaseDamage)
            {
                API.SetVehicleDamage(Fx.Handle, 0f, 1.2f, 0f, 1600f, 1600f, true);
                API.SetVehicleDamage(Fx.Handle, 0f, 0.75f, 0.05f, 1600f, 1600f, true);
                API.SetVehicleDamage(Fx.Handle, -0.7f, 0f, 0f, 1600f, 1600f, true);
                API.SetVehicleDamage(Fx.Handle, 0.7f, 0f, 0f, 1600f, 1600f, true);
            }
        }

        public void ParticleEffect(string dict, string fx, Vector3 offset, float scale)
        {
            BaseScript.TriggerServerEvent("s:mm:particle", Fx.NetworkId, dict, fx, offset.X, offset.Y, offset.Z, scale);
        }
    }

    public enum Wheels
    {
        FRONT_LEFT = 0,
        FRONT_RIGHT = 1,
        MID_LEFT = 2,
        MID_RIGHT = 3,
        REAR_LEFT = 4,
        REAR_RIGHT = 5,
        TRAILER_MID_LEFT = 45,
        TRAILER_MID_RIGHT = 46,
    }
}
