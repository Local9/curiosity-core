using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.ClientEvents;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Classes
{
    [Serializable]
    public class GangMember : Entity, IEquatable<GangMember>
    { 
        public CitizenFX.Core.Ped Fx { get; set; }
        internal PluginManager Instance => PluginManager.Instance;
        internal EventSystem EventSystem => EventSystem.GetModule();
        public Tasks Task => Fx.Task;
        internal string Name => Fx.Model.ToString();
        internal DateTime DateCreated { get; set; }
        public bool IsInVehicle => Fx.IsInVehicle();
        private bool _DEBUG_ENABLED { get; set; } = false;
        public DateTime LastUpdate { get; private set; }
        public string Identity { get; internal set; }

        private EntityEventWrapper _eventWrapper;
        private DateTime TimeOfDeath = new DateTime(1900, 1, 1);
        private bool isRemoving;

        public GangMember(CitizenFX.Core.Ped fx) : base(fx.Handle)
        {
            Fx = fx;

            API.NetworkRegisterEntityAsNetworked(fx.Handle);
            API.NetworkRequestControlOfEntity(fx.Handle);

            this._eventWrapper = new EntityEventWrapper(this.Fx);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            fx.Health = 200;

            fx.AlwaysDiesOnLowHealth = false;
            fx.DropsWeaponsOnDeath = false;
            fx.CanWrithe = false;

            DateCreated = DateTime.Now;

            Fx.AlwaysKeepTask = true;
            Fx.IsPersistent = true;

            API.SetPedFleeAttributes(Fx.Handle, 0, false);
            API.SetBlockingOfNonTemporaryEvents(Fx.Handle, true);
            API.TaskSetBlockingOfNonTemporaryEvents(Fx.Handle, true);
            API.SetPedCombatAttributes(Fx.Handle, 17, false); // Flee if faced with weapon
            API.SetPedCombatAttributes(Fx.Handle, 46, false); // BF_AlwaysFight 
            API.SetPedCombatAttributes(Fx.Handle, 5, false); // BF_CanFightArmedPedsWhenNotArmed 

            Fx.SetConfigFlag(281, true); // No more rolling about
            API.SetPedDiesInWater(Fx.Handle, false);
            API.SetPedDiesWhenInjured(Fx.Handle, false);

            int rand = Utility.RANDOM.Next(4);

            fx.State.Set(StateBagKey.PED_SETUP, true, true);
            fx.State.Set(StateBagKey.MENU_RANDOM_RESPONSE, rand, true);
        }

        internal async void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            bool flag;

            if (this.Position.VDist(Cache.PlayerPed.Position) <= 120f || Fx.IsPersistent)
            {
                flag = false;
            }
            else
            {
                flag = (!base.IsOnScreen ? true : base.IsDead && !Fx.IsPersistent);
            }
            if (flag)
            {
                base.Delete();
            }

            if (TimeOfDeath.Year != 1900) // Remove the ped from the world
            {
                if (DateTime.Now.Subtract(TimeOfDeath).TotalSeconds > 5)
                {
                    Dismiss();
                    return;
                }
            }

            bool isNpcDebug = Cache.PlayerPed.State.Get(StateBagKey.PLAYER_DEBUG_NPC) ?? false;

            if (isNpcDebug && !_DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                Instance.AttachTickHandler(OnDeveloperOverlay);
                _DEBUG_ENABLED = true;
                Logger.Debug($"PED Debug Enabled");
            }
            else if (!isNpcDebug && _DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                _DEBUG_ENABLED = false;
                Instance.DetachTickHandler(OnDeveloperOverlay);
                Logger.Debug($"PED Debug Disabled");
            }
        }

        async Task OnDeveloperOverlay()
        {
            this.DrawData();
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            Dismiss();
        }

        private async void OnDied(EntityEventWrapper sender, Entity entity)
        {
            TimeOfDeath = DateTime.Now;

            Blip b = Fx.AttachedBlip;

            if (b != null)
            {
                if (b.Exists())
                    b.Delete();
            }

            Dismiss();
        }

        public async void Dismiss()
        {
            if (Fx == null) return;
            if (!base.Exists()) return;

            if (isRemoving) return;
            isRemoving = true;

            int handle = Fx.Handle;
            API.RemovePedElegantly(ref handle);

            Fx.Detach();

            await Fx.FadeOut();

            Blip singleBlip = Fx.AttachedBlip;
            if (singleBlip != null)
            {
                if (singleBlip.Exists())
                    singleBlip.Delete();
            }

            Fx.IsPersistent = false;
            Fx.MarkAsNoLongerNeeded();

            EventSystem.Send("delete:entity", Fx.NetworkId);

            base.Delete();
        }

        protected bool Equals(GangMember other)
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
                flag = this != (Entity)obj ? obj.GetType() != GetType() ? false : Equals((GangMember)obj) : true;
            }
            return flag;
        }

        public void ParticleEffect(string dict, string fx, Vector3 offset, float scale)
        {
            EntityHandler.ParticleEffect(NetworkId, dict, fx, offset, scale);
        }

        bool IEquatable<GangMember>.Equals(GangMember other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Fx, other.Fx));
        }
    }
}
