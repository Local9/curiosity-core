using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.Wrappers;
using System;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Missions.Client.net.DataClasses;
// INTERACTIONS
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;
using Curiosity.Shared.Client.net;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class InteractivePed : Entity, IEquatable<Ped>
    {
        private static Client client = Client.GetInstance();

        public const int MovementUpdateInterval = 5;
        private const string MOVEMENT_ANIMATION_SET_DRUNK = "MOVE_M@DRUNK@VERYDRUNK";
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
        public bool IsUsingCannabis;
        public bool IsUsingCocaine;
        public bool IsAllowedToBeSearched;
        public bool HasLostId;
        public bool HasAskedForId;
        public bool HasBeenSearched;
        // MENU STATES
        private bool IsCoronerCalled;
        // Settings
        public virtual string MovementStyle { get; set; }
        public virtual bool PlayAudio { get; set; }
        // Events
        private EntityEventWrapper _eventWrapper;
        private bool _IsAttackingTarget;

        private static string helpText;

        private string _Firstname, _Surname, _DateOfBirth;
        private int _Attitude, _BloodAlcaholLimit, _ChanceOfFlee, _ChanceOfShootAndFlee;

        // PED INFORMATION
        public string Name
        {
            get
            {
                return $"{_Firstname} {_Surname}";
            }
        }
        public string DateOfBirth
        {
            get
            {
                return _DateOfBirth;
            }
        }
        public int Attitude
        {
            get
            {
                return _Attitude;
            }
        }

        public int BloodAlcaholLimit
        {
            get
            {
                return _BloodAlcaholLimit;
            }
        }

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
            HasAskedForId = false;

            IsUsingCannabis = false;
            IsUsingCocaine = false;
            HasLostId = false;
            HasBeenSearched = false;

            _ChanceOfFlee = 0;
            _ChanceOfShootAndFlee = 0;

            this.Ped.Health = 200;
            this.Ped.IsPersistent = true;

            _Firstname = string.Empty;
            _Surname = string.Empty;

            if (this.Ped.Gender == Gender.Female)
            {
                _Firstname = PedNames.FirstNameFemale[Client.Random.Next(PedNames.FirstNameFemale.Count)];
            }
            else
            {
                _Firstname = PedNames.FirstNameMale[Client.Random.Next(PedNames.FirstNameMale.Count)];
            }
            _Surname = PedNames.Surname[Client.Random.Next(PedNames.Surname.Count)];

            DateTime StartDateForDriverDoB = new DateTime(1949, 1, 1);
            double Range = (DateTime.Today - StartDateForDriverDoB).TotalDays;
            Range = Range - 6570; // MINUS 18 YEARS
            _DateOfBirth = StartDateForDriverDoB.AddDays(Client.Random.Next((int)Range)).ToString("yyyy-MM-dd");

            _Attitude = Client.Random.Next(100);

            int breathlyzerLimit = Client.Random.Next(100);
            _BloodAlcaholLimit = 0;
            IsUnderTheInfluence = false;
            IsAllowedToBeSearched = false;

            if (breathlyzerLimit >= 60)
            {
                _BloodAlcaholLimit = Client.Random.Next(1, 7);
                if (breathlyzerLimit >= 88)
                {
                    IsUnderTheInfluence = true;
                    IsAllowedToBeSearched = true;
                    CanBeArrested = true;
                    _BloodAlcaholLimit = Client.Random.Next(8, 10);
                    _ChanceOfFlee = Client.Random.Next(25, 30);

                    if (breathlyzerLimit >= 95)
                    {
                        _BloodAlcaholLimit = Client.Random.Next(10, 20);
                        _ChanceOfShootAndFlee = Client.Random.Next(1, 5);
                    }
                }
            }

            if (Client.Random.Next(100) >= 85)
            {
                IsUsingCannabis = true; // Its weed ffs
                IsAllowedToBeSearched = true;
            }

            if (Client.Random.Next(100) >= 90)
            {
                IsUsingCocaine = true;
                CanBeArrested = true;
                IsAllowedToBeSearched = true;
            }

            if (Client.Random.Next(100) >= 95)
            {
                HasLostId = true;
                IsAllowedToBeSearched = true;
            }

            if (Client.Random.Next(100) >= 95)
            {
                IsAllowedToBeSearched = true;
            }

            Create();
        }

        private async void Create()
        {
            this._eventWrapper = new EntityEventWrapper(this.Ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            client.RegisterEventHandler("curiosity:interaction:idRequesed", new Action<int>(OnIdRequested));
            client.RegisterEventHandler("curiosity:interaction:cpr", new Action<int, bool>(OnCpr));
            client.RegisterEventHandler("curiosity:interaction:cpr:failed", new Action<int>(OnCprFailed));
            client.RegisterEventHandler("curiosity:interaction:coroner", new Action<int>(OnCoronerCalled));
            client.RegisterEventHandler("curiosity:interaction:searched", new Action<int, bool>(OnPedHasBeenSearched));

            client.DeregisterTickHandler(OnMenuTask);
            client.DeregisterTickHandler(OnShowHelpTextTask);

            client.RegisterTickHandler(OnShowHelpTextTask);
            client.RegisterTickHandler(OnMenuTask);

            if (_ChanceOfShootAndFlee == 4)
            {
                WeaponHash weaponHash = WeaponHash.Pistol;
                if (Client.Random.Next(5) == 4)
                {
                    weaponHash = WeaponHash.SawnOffShotgun;
                }
                this.Ped.Weapons.Give(weaponHash, 1, true, true);
                this.Ped.Task.ShootAt(Game.PlayerPed);
                await Client.Delay(3000);
                this.Ped.Task.FleeFrom(Game.PlayerPed);
            }
            else if (_ChanceOfFlee >= 28)
            {
                this.Ped.Task.FleeFrom(Game.PlayerPed);
            }

            if (IsUnderTheInfluence)
            {
                if (!HasAnimSetLoaded(MOVEMENT_ANIMATION_SET_DRUNK))
                {
                    RequestAnimSet(MOVEMENT_ANIMATION_SET_DRUNK);
                    while (!HasAnimSetLoaded(MOVEMENT_ANIMATION_SET_DRUNK))
                    {
                        await Client.Delay(0);
                    }
                }
                this.Ped.MovementAnimationSet = MOVEMENT_ANIMATION_SET_DRUNK;
            }
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
            if (this.Ped.IsBeingStunned)
            {
                this.Ped.Health = 200;

                if (Client.Random.Next(5) >= 3 && !IsArrested)
                {
                    IsArrested = true;
                    ArrestInteractions.InteractionArrestInit(this);
                }
            }

            if (this.Ped.Position.Distance(Game.PlayerPed.Position) >= 20f && IsArrested && !this.Ped.IsInVehicle())
            {
                IsArrested = false;
                this.Ped.LeaveGroup();
                this.Ped.Task.FleeFrom(Game.PlayerPed);
            }

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

                if (Game.PlayerPed.IsAiming && Ped.IsAlive && !IsArrested) {
                    if (this.Position.Distance(Game.PlayerPed.Position) > 2f && this.Position.Distance(Game.PlayerPed.Position) <= 20f)
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
            try
            {
                if (Scripts.Menus.PedInteractionMenu.MenuBase.MainMenu != null)
                    IsMenuVisible = Scripts.Menus.PedInteractionMenu.MenuBase.AnyMenuVisible();

                //if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                //    Screen.ShowSubtitle($"Menu: {IsMenuVisible}, CPR: {IsPerformingCpr}");

                return !IsMenuVisible && !IsPerformingCpr && !IsCoronerCalled && !Game.PlayerPed.IsInVehicle();
            }
            catch (Exception ex)
            {
                Log.Error("InteractivePed -> CanDisplayMenu");
                return false;
            }
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

        public void OnIdRequested(int networkId)
        {
            if (Ped.NetworkId == networkId)
                HasAskedForId = true;
        }

        public void OnPedHasBeenSearched(int networkId, bool illegalItems)
        {
            if (Ped.NetworkId == networkId)
            {
                HasBeenSearched = true;
                IsCarryingIllegalItems = illegalItems;
            }
        }

        public event InteractivePed.OnAttackingTargetEvent AttackTarget;
        public delegate void OnAttackingTargetEvent(Ped target);
    }
}
