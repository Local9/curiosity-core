using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net.NPC;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using System;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class WorldPed : Entity, IEquatable<Ped>
    {
        // RAGE ENGINE
        private EntityEventWrapper _eventWrapper;

        public readonly Ped Ped;
        public readonly Vehicle Vehicle;

        public NpcProfile Profile;

        static WorldPed()
        {
        }

        protected WorldPed(int handle) : base(handle)
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper() && Client.DeveloperNpcUiEnabled)
            {
                Screen.ShowNotification($"~r~[~g~D~b~E~y~V~o~]~w~ Creating World Ped");
            }

            Ped = new Ped(handle);
            Wrappers.Helpers.RequestControlOfEnt(Ped);

            this._eventWrapper = new EntityEventWrapper(Ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(Abort);

            if (Ped.IsInVehicle())
            {
                Vehicle = this.Ped.CurrentVehicle;
            }

            int netId = API.NetworkGetNetworkIdFromEntity(Ped.Handle);
            Decorators.Set(Ped.Handle, Decorators.DECOR_PED_NETWORKID, netId);
            API.SetNetworkIdCanMigrate(netId, true);

            Decorators.Set(Ped.Handle, Decorators.DECOR_PED_INTERACTIVE, true);
            Decorators.Set(Ped.Handle, Decorators.DECOR_PED_MISSION, true);

            API.SetPedFleeAttributes(Ped.Handle, 0, false);
            API.SetBlockingOfNonTemporaryEvents(Ped.Handle, true);
            API.TaskSetBlockingOfNonTemporaryEvents(Ped.Handle, true);
            API.SetPedDiesInWater(Ped.Handle, false);
            API.SetPedDiesWhenInjured(Ped.Handle, false);
            API.SetPedDiesInstantlyInWater(Ped.Handle, true);

            Ped.SetCombatAttributes((CombatAttributes)17, false);
            Ped.SetCombatAttributes((CombatAttributes)46, false);
            Ped.SetCombatAttributes((CombatAttributes)5, false);

            Ped.Health = 200;
            Ped.IsPersistent = true;
        }

        private void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();
        }

        private void Update(EntityEventWrapper sender, Entity entity)
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

        public bool Equals(WorldPed other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Ped, other.Ped));
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
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((WorldPed)obj) : true;
            }
            return flag;
        }

        public bool Equals(Ped other)
        {
            return object.Equals(this.Ped, other);
        }

        public static implicit operator Ped(WorldPed v)
        {
            return v.Ped;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this.Ped != null ? this.Ped.GetHashCode() : 0);
        }

        public event WorldPed.OnAttackingTargetEvent AttackTarget;
        public event WorldPed.OnGoingToTargetEvent GoToTarget;

        public delegate void OnAttackingTargetEvent(Ped target);
        public delegate void OnGoingToTargetEvent(Ped target);
    }
}
