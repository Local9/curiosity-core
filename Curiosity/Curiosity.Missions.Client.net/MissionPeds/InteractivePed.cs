using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.Wrappers;
using System;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Shared.Client.net.Extensions;
// INTERACTIONS
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class InteractivePed : Entity, IEquatable<Ped>
    {
        private static Client client = Client.GetInstance();

        public const int MovementUpdateInterval = 5;

        public readonly Ped Ped;
        private Ped _target;
        // MENU STATES
        public bool IsMenuVisible = false;
        public bool IsTrafficStop;
        public bool IsPerformingCpr = false;
        public bool HasCprFailed = false;
        // Safe
        public bool IsVictim;
        public bool IsHostage;
        // Evil
        public bool CanBeArrested;
        public bool IsHandcuffed;
        public bool IsArrested;
        // Arrested States
        public bool IsUnderTheInfluence;
        public bool IsCarryingIllegalItems;
        public bool IsCarryingStolenItems;
        // MENU STATES
        private bool IsCoronerCalled;
        // Settings
        public virtual string MovementStyle { get; set; }
        public virtual bool PlayAudio { get; set; }
        // Events
        private EntityEventWrapper _eventWrapper;
        private bool _IsAttackingTarget;

        private static string helpText;

        static InteractivePed()
        {
        }

        protected InteractivePed(int handle) : base(handle)
        {
            this.Ped = new Ped(handle);

            IsMenuVisible = false;
            IsPerformingCpr = false;
            IsCoronerCalled = false;
            HasCprFailed = false;
            IsArrested = false;

            Create();
        }

        private void Create()
        {
            this._eventWrapper = new EntityEventWrapper(this.Ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            client.RegisterEventHandler("curiosity:interaction:cpr", new Action<int, bool>(OnCpr));
            client.RegisterEventHandler("curiosity:interaction:cpr:failed", new Action<int>(OnCprFailed));
            client.RegisterEventHandler("curiosity:interaction:coroner", new Action<int>(OnCoronerCalled));

            client.DeregisterTickHandler(OnMenuTask);
            client.DeregisterTickHandler(OnShowHelpTextTask);

            client.RegisterTickHandler(OnShowHelpTextTask);
            client.RegisterTickHandler(OnMenuTask);
        }

        protected bool Equals(InteractivePed other)
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
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((InteractivePed)obj) : true;
            }
            return flag;
        }

        public bool Equals(Ped other)
        {
            return object.Equals(this.Ped, other);
        }

        public static implicit operator Ped(InteractivePed v)
        {
            return v.Ped;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this.Ped != null ? this.Ped.GetHashCode() : 0);
        }

        private async Task OnShowHelpTextTask()
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
            client.DeregisterTickHandler(OnShowHelpTextTask);
        }

        private async Task OnMenuTask()
        {
            await Task.FromResult(0);
            helpText = string.Empty;

            if (Scripts.MissionEvents.HasAcceptedCallout) return; // Should also check at the traffic stop

            if (this.Position.VDist(Game.PlayerPed.Position) <= 120f)
            {
                if (this.Position.Distance(Game.PlayerPed.Position) <= 2f && CanDisplayMenu())
                {
                    helpText = "Press ~INPUT_CONTEXT~ to open the ~b~Interaction Menu";

                    if (Game.IsControlPressed(0, Control.Context))
                    {
                        helpText = string.Empty;
                        Scripts.Menus.PedInteractionMenu.MenuBase.Open(this);
                    }
                }

                if (Game.PlayerPed.IsAiming && Ped.IsAlive) {
                    if (this.Position.Distance(Game.PlayerPed.Position) > 4f && this.Position.Distance(Game.PlayerPed.Position) <= 20f && !IsArrested)
                    {
                        int entityHandle = 0;
                        Ped pedBeingAimedAt = null;
                        if (API.GetEntityPlayerIsFreeAimingAt(Game.Player.Handle, ref entityHandle))
                        {
                            if (entityHandle == 0) return;

                            if (API.GetEntityType(entityHandle) == 1 && API.GetPedType(entityHandle) != 28)
                            {
                                pedBeingAimedAt = new Ped(entityHandle);
                            }
                        }

                        if (pedBeingAimedAt != null)
                        {
                            if (Ped == pedBeingAimedAt)
                            {
                                if (Ped.IsInVehicle())
                                {
                                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to demand the suspect to exit their vehicle.");
                                }
                                else
                                {
                                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to demand the suspect to get on their knees.");
                                }

                                if (Game.IsControlJustPressed(0, Control.Context))
                                {
                                    IsArrested = true;
                                    ArrestInteractions.InteractionArrestInit(this);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CanDisplayMenu()
        {
            if (Scripts.Menus.PedInteractionMenu.MenuBase.MainMenu != null)
                IsMenuVisible = Scripts.Menus.PedInteractionMenu.MenuBase.MainMenu.Visible;

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                Screen.ShowSubtitle($"Menu: {IsMenuVisible}, CPR: {IsPerformingCpr}");

            return !IsMenuVisible && !IsPerformingCpr && !IsCoronerCalled && !Game.PlayerPed.IsInVehicle();
        }

        private void OnCpr(int networkId, bool state)
        {
            if (Ped.NetworkId == networkId)
                IsPerformingCpr = state;
        }

        private void OnCprFailed(int networkId)
        {
            if (Ped.NetworkId == networkId)
            {
                IsPerformingCpr = false;
                HasCprFailed = true;
            }
        }

        private void OnCoronerCalled(int networkId)
        {
            if (Ped.NetworkId == networkId)
                IsCoronerCalled = true;
        }

        public event InteractivePed.OnAttackingTargetEvent AttackTarget;
        public delegate void OnAttackingTargetEvent(Ped target);
    }
}
