using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.Wrappers;
using System;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class InteractivePed : Entity, IEquatable<Ped>
    {
        private static Client client = Client.GetInstance();

        public const int MovementUpdateInterval = 5;

        private readonly Ped _ped;
        private Ped _target;
        // MENU STATES
        public bool IsInteracting;
        public bool IsTrafficStop;
        public bool IsPerformingCpr;
        // Safe
        public bool IsVictim;
        public bool IsHostage;
        // Evil
        public bool CanBeArrested;
        public bool IsHandcuffed;
        // Arrested States
        public bool IsUnderTheInfluence;
        public bool IsCarryingIllegalItems;
        public bool IsCarryingStolenItems;
        // Settings
        public virtual string MovementStyle { get; set; }
        public virtual bool PlayAudio { get; set; }
        // Events
        private EntityEventWrapper _eventWrapper;
        private bool _IsAttackingTarget;

        private static string helpText;

        static InteractivePed()
        {
            client.RegisterTickHandler(OnShowHelpTextTask);
        }

        protected InteractivePed(int handle) : base(handle)
        {
            this._ped = new Ped(handle);
            Create();
        }

        private void Create()
        {
            this._eventWrapper = new EntityEventWrapper(this._ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            client.RegisterTickHandler(OnMenuTask);
        }

        protected bool Equals(InteractivePed other)
        {
            return (!base.Equals(other) ? false : object.Equals(this._ped, other._ped));
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
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((InteractivePed)obj) : true;
            }
            return flag;
        }

        public bool Equals(Ped other)
        {
            return object.Equals(this._ped, other);
        }

        public static implicit operator Ped(InteractivePed v)
        {
            return v._ped;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this._ped != null ? this._ped.GetHashCode() : 0);
        }

        private static async Task OnShowHelpTextTask()
        {
            await Task.FromResult(0);
            if (!string.IsNullOrEmpty(helpText))
                Screen.DisplayHelpTextThisFrame(helpText);
        }
        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
            client.DeregisterTickHandler(OnMenuTask);
        }

        public void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            //bool flag;
            //if (this.Position.VDist(Game.PlayerPed.Position) <= 120f)
            //{
            //    flag = false;
            //}
            //else
            //{
            //    flag = (!base.IsOnScreen ? true : base.IsDead);
            //}
            //if (flag)
            //{
            //    base.Delete();
            //}
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();
            client.DeregisterTickHandler(OnMenuTask);
        }

        private async Task OnMenuTask()
        {
            await Task.FromResult(0);
            helpText = string.Empty;
            if (this.Position.VDist(Game.PlayerPed.Position) <= 120f)
            {
                if (Scripts.Menus.PedInteractionMenu.MenuBase.MainMenu != null)
                    IsInteracting = Scripts.Menus.PedInteractionMenu.MenuBase.MainMenu.Visible;

                if (this.Position.Distance(Game.PlayerPed.Position) <= 2f && !IsInteracting && !IsPerformingCpr)
                {
                    helpText = "Press ~INPUT_CONTEXT~ to open the ~b~Interaction Menu";

                    if (Game.IsControlPressed(0, Control.Context))
                    {
                        helpText = string.Empty;
                        Scripts.Menus.PedInteractionMenu.MenuBase.Open(this);
                    }
                }
            }
        }

        public event InteractivePed.OnAttackingTargetEvent AttackTarget;
        public delegate void OnAttackingTargetEvent(Ped target);
    }
}
