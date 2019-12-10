using CitizenFX.Core;
using Curiosity.Missions.Client.net.Wrappers;
using System;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Curiosity.Missions.Client.net.MissionVehicles
{
    abstract class InteractiveVehicle : Entity, IEquatable<Vehicle>
    {
        private static Client client = Client.GetInstance();

        public readonly Vehicle Vehicle;
        private EntityEventWrapper _eventWrapper;

        private string helpText = string.Empty;

        // Vehicle States

        public InteractiveVehicle(int handle) : base(handle)
        {
            this.Vehicle = new Vehicle(handle);

            Create();
        }

        public void Create()
        {
            this._eventWrapper = new EntityEventWrapper(this.Vehicle);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            client.RegisterTickHandler(OnShowHelpTextTask);
        }

        protected bool Equals(InteractiveVehicle other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Vehicle, other.Vehicle));
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
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((InteractiveVehicle)obj) : true;
            }
            return flag;
        }

        public bool Equals(Vehicle other)
        {
            return object.Equals(this.Vehicle, other);
        }

        public static implicit operator Vehicle(InteractiveVehicle v)
        {
            return v.Vehicle;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this.Vehicle != null ? this.Vehicle.GetHashCode() : 0);
        }

        private async Task OnShowHelpTextTask()
        {
            await Task.FromResult(0);
            if (!string.IsNullOrEmpty(helpText))
                Screen.DisplayHelpTextThisFrame(helpText);
        }

        public void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {

        }

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();
            
            client.DeregisterTickHandler(OnShowHelpTextTask);
        }
    }
}
