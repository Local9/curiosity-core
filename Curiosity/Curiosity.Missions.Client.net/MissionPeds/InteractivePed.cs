using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Entity;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.Extensions;
using Curiosity.Missions.Client.Managers;
using Curiosity.Missions.Client.Scripts;
// INTERACTIONS
using Curiosity.Missions.Client.Scripts.Interactions.PedInteractions;
using Curiosity.Missions.Client.Static;
using Curiosity.Missions.Client.Utils;
using Curiosity.Missions.Client.Wrappers;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.MissionPeds
{
    abstract class InteractivePed : Entity, IEquatable<Ped>
    {
        private static PluginManager PluginInstance => PluginManager.Instance;

        public const int MovementUpdateInterval = 10;
        private const string MOVEMENT_ANIMATION_SET_DRUNK = "MOVE_M@DRUNK@VERYDRUNK";
        public readonly Ped Ped;
        private Ped _target;
        private Vehicle Vehicle;
        // MENU STATES

        public bool IsTrafficStop;
        public bool IsPerformingCpr = false;
        public bool HasCprFailed = false;
        public bool HasIdentifcationBeenRan;
        // Safe
        public bool IsVictim;
        public bool IsHostage;
        // Evil
        public bool CanBeArrested
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_CAN_BE_ARRESTED);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_CAN_BE_ARRESTED, value);
            }
        }

        public bool IsHandcuffed
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_HANDCUFF);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_HANDCUFF, value);
            }
        }
        public bool IsArrested
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_ARRESTED);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_ARRESTED, value);
            }
        }

        // Arrested States
        public bool IsUnderTheInfluence
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_DRUG_ALCOHOL);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_DRUG_ALCOHOL, value);
            }
        }

        public bool IsCarryingIllegalItems
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_ITEM_ILLEGAL);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_ITEM_ILLEGAL, value);
            }
        }

        public bool IsCarryingStolenItems
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_ITEM_STOLEN);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_ITEM_STOLEN, value);
            }
        }

        public bool IsUsingCannabis
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_DRUG_CANNABIS);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_DRUG_CANNABIS, value);
            }
        }

        public bool IsUsingCocaine
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_DRUG_COCAINE);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_DRUG_COCAINE, value);
            }
        }

        public bool IsAllowedToBeSearched
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_INTERACTION_CAN_BE_SEARCHED);
            }
            set
            {
                Set(PluginManager.DECOR_INTERACTION_CAN_BE_SEARCHED, value);
            }
        }

        public bool HasBeenSearched
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_INTERACTION_HAS_BEEN_SEARCHED);
            }
            set
            {
                Set(PluginManager.DECOR_INTERACTION_HAS_BEEN_SEARCHED, value);
            }
        }

        public bool HasLostId
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_INTERACTION_LOST_ID);
            }
            set
            {
                Set(PluginManager.DECOR_INTERACTION_LOST_ID, value);
            }
        }

        public bool HasProvidedId
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_INTERACTION_PROVIDED_ID);
            }
            set
            {
                Set(PluginManager.DECOR_INTERACTION_PROVIDED_ID, value);
            }
        }

        public bool HasBeenGrabbed
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_INTERACTION_GRABBED);
            }
            set
            {
                Set(PluginManager.DECOR_INTERACTION_GRABBED, value);
            }
        }

        public bool HasStolenCar
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_VEHICLE_STOLEN);
            }
            set
            {
                Set(PluginManager.DECOR_VEHICLE_STOLEN, value);
            }
        }

        public bool CaughtSpeeding
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_VEHICLE_SPEEDING);
            }
            set
            {
                Set(PluginManager.DECOR_VEHICLE_SPEEDING, value);
            }
        }

        public bool IsWanted
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_INTERACTION_WANTED);
            }
            set
            {
                Set(PluginManager.DECOR_INTERACTION_WANTED, value);
            }
        }

        public bool RanFromPolice
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_NPC_RAN_FROM_POLICE);
            }
            set
            {
                Set(PluginManager.DECOR_NPC_RAN_FROM_POLICE, value);
            }
        }

        // MENU STATES
        private bool IsCoronerCalled
        {
            get
            {
                return GetBoolean(PluginManager.DECOR_INTERACTION_CORONER_CALLED);
            }
            set
            {
                Set(PluginManager.DECOR_INTERACTION_CORONER_CALLED, value);
            }
        }

        // Settings
        public virtual string MovementStyle { get; set; }
        public virtual bool PlayAudio { get; set; }
        // Events
        private EntityEventWrapper _eventWrapper;
        private bool _IsAttackingTarget, _hasBeenReleased, _stolenCar;

        private static string helpText;

        private string _firstname, _surname, _dateOfBirth, _offence;
        private int _attitude, _bloodAlcaholLimit, _chanceOfFlee, _chanceOfShootAndFlee, _numberOfCitations;
        private DateTime _currentMovementUpdateTime;

        // fighting information
        private bool _goingToTarget;
        private bool _attackingTarget;
        public static float SensingRange;
        public static float SilencerEffectiveRange;
        public static float BehindNoticeDistance;
        public static float RunningNoticeDistance;
        // PED INFORMATION
        public static float WanderRadius;
        public static float VisionDistance;
        public static float AttackRange;

        public string Name
        {
            get
            {
                return $"{_firstname} {_surname}";
            }
        }
        public string DateOfBirth
        {
            get
            {
                return _dateOfBirth;
            }
        }
        public int Attitude
        {
            get
            {
                return _attitude;
            }
        }

        public int BloodAlcaholLimit
        {
            get
            {
                return _bloodAlcaholLimit;
            }
        }

        public int NumberOfCitations
        {
            get
            {
                return _numberOfCitations;
            }
        }

        public string Offence
        {
            get
            {
                return _offence;
            }
        }

        public bool AttackingTarget
        {
            get
            {
                return this._attackingTarget;
            }
            set
            {
                if ((!value || this.Ped.IsRagdoll || base.IsDead || this.Ped.IsClimbing || this.Ped.IsFalling || this.Ped.IsBeingStunned ? false : !this.Ped.IsGettingUp))
                {
                    InteractivePed.OnAttackingTargetEvent onAttackingTargetEvent = this.AttackTarget;
                    if (onAttackingTargetEvent != null)
                    {
                        onAttackingTargetEvent(this.Target);
                    }
                }
                this._attackingTarget = value;
            }
        }

        public Ped Target
        {
            get
            {
                return this._target;
            }
            private set
            {
                if ((value != null ? false : this._target != null))
                {
                    this.Ped.Task.WanderAround(this.Position, InteractivePed.WanderRadius);
                    int num = 0;
                    bool flag = num == 1;
                    this.AttackingTarget = num == 1;
                    this.GoingToTarget = flag;
                }
                this._target = value;
            }
        }

        public bool GoingToTarget
        {
            get
            {
                return this._goingToTarget;
            }
            set
            {
                if ((!value ? false : !this._goingToTarget))
                {
                    InteractivePed.OnGoingToTargetEvent onGoingToTargetEvent = this.GoToTarget;
                    if (onGoingToTargetEvent != null)
                    {
                        onGoingToTargetEvent(this.Target);
                    }
                    else
                    {
                    }
                }
                this._goingToTarget = value;
            }
        }

        static InteractivePed()
        {
        }

        protected InteractivePed(int handle) : base(handle)
        {
            if (PlayerManager.IsDeveloper && PluginManager.DeveloperNpcUiEnabled)
            {
                Screen.ShowNotification($"~r~[~g~D~b~E~y~V~o~]~w~ Creating Interactive Ped");
            }

            if (DecorGetBool(handle, PluginManager.DECOR_NPC_WAS_RELEASED))
            {
                Screen.ShowNotification("~r~This pedestrian was recently released.");
                return;
            }

            this.Ped = new Ped(handle);

            Decorators.Set(this.Ped.Handle, Decorators.PED_INTERACTIVE, true);

            if (this.Ped.IsInVehicle())
                this.Vehicle = this.Ped.CurrentVehicle;

            int netId = API.NetworkGetNetworkIdFromEntity(this.Ped.Handle);
            SetNetworkIdCanMigrate(netId, true);

            Wrappers.Helpers.RequestControlOfEnt(this.Ped);

            API.SetPedFleeAttributes(Ped.Handle, 0, false);
            API.SetBlockingOfNonTemporaryEvents(Ped.Handle, true);
            API.TaskSetBlockingOfNonTemporaryEvents(Ped.Handle, true);
            API.SetPedDiesInWater(Ped.Handle, false);
            API.SetPedDiesWhenInjured(Ped.Handle, false);
            API.SetPedDiesInstantlyInWater(Ped.Handle, true);

            Ped.SetCombatAttributes((CombatAttributes)17, false);
            Ped.SetCombatAttributes((CombatAttributes)46, false);
            Ped.SetCombatAttributes((CombatAttributes)5, false);

            IsPerformingCpr = false;
            IsCoronerCalled = false;
            HasCprFailed = false;
            IsArrested = false;
            HasProvidedId = false;

            IsUsingCannabis = false;
            IsUsingCocaine = false;
            HasLostId = false;
            HasBeenSearched = false;
            HasIdentifcationBeenRan = false;

            _chanceOfFlee = 0;
            _chanceOfShootAndFlee = 0;

            this.Ped.Health = 200;
            this.Ped.IsPersistent = true;

            VisionDistance = 50f;
            AttackRange = 50f;
            WanderRadius = 50f;

            _firstname = string.Empty;
            _surname = string.Empty;

            if (this.Ped.Gender == Gender.Female)
            {
                _firstname = PedNames.FirstNameFemale[PluginManager.Random.Next(PedNames.FirstNameFemale.Count)];
            }
            else
            {
                _firstname = PedNames.FirstNameMale[PluginManager.Random.Next(PedNames.FirstNameMale.Count)];
            }
            _surname = PedNames.Surname[PluginManager.Random.Next(PedNames.Surname.Count)];

            DateTime StartDateForDriverDoB = new DateTime(1949, 1, 1);
            double Range = (DateTime.Today - StartDateForDriverDoB).TotalDays;
            Range = Range - 6570; // MINUS 18 YEARS
            _dateOfBirth = StartDateForDriverDoB.AddDays(PluginManager.Random.Next((int)Range)).ToString("yyyy-MM-dd");

            _attitude = PluginManager.Random.Next(100);


            if (IsUnderTheInfluence)
            {
                int breathlyzerLimit = PluginManager.Random.Next(60, 100);
                _bloodAlcaholLimit = 0;

                if (breathlyzerLimit >= 60)
                {
                    _bloodAlcaholLimit = PluginManager.Random.Next(1, 7);
                    if (breathlyzerLimit >= 88)
                    {
                        IsUnderTheInfluence = true;
                        IsAllowedToBeSearched = true;
                        CanBeArrested = true;
                        _bloodAlcaholLimit = PluginManager.Random.Next(8, 10);
                        _chanceOfFlee = PluginManager.Random.Next(25, 30);

                        if (breathlyzerLimit >= 95)
                        {
                            _bloodAlcaholLimit = PluginManager.Random.Next(10, 20);
                            _chanceOfShootAndFlee = PluginManager.Random.Next(1, 5);
                        }
                    }
                }

            }
            else
            {
                int breathlyzerLimit = PluginManager.Random.Next(100);
                _bloodAlcaholLimit = 0;
                IsUnderTheInfluence = false;
                IsAllowedToBeSearched = false;

                if (breathlyzerLimit >= 60)
                {
                    _bloodAlcaholLimit = PluginManager.Random.Next(1, 7);
                    if (breathlyzerLimit >= 88)
                    {
                        IsUnderTheInfluence = true;
                        IsAllowedToBeSearched = true;
                        CanBeArrested = true;
                        _bloodAlcaholLimit = PluginManager.Random.Next(8, 10);
                        _chanceOfFlee = PluginManager.Random.Next(25, 30);

                        if (breathlyzerLimit >= 95)
                        {
                            _bloodAlcaholLimit = PluginManager.Random.Next(10, 20);
                            _chanceOfShootAndFlee = PluginManager.Random.Next(1, 5);
                        }
                    }
                }
            }

            if (PluginManager.Random.Next(100) >= 85)
            {
                IsUsingCannabis = true; // Its weed ffs
                CanBeArrested = true;
                IsAllowedToBeSearched = true;
            }

            if (PluginManager.Random.Next(100) >= 90)
            {
                IsUsingCocaine = true;
                CanBeArrested = true;
                IsAllowedToBeSearched = true;
            }

            if (PluginManager.Random.Next(100) >= 95)
            {
                HasLostId = true;
                IsAllowedToBeSearched = true;
            }

            if (PluginManager.Random.Next(100) >= 95)
            {
                IsAllowedToBeSearched = true;
            }

            _numberOfCitations = PluginManager.Random.Next(8);

            _offence = "~g~NONE";
            if (PluginManager.Random.Next(100) >= 75)
            {
                List<string> Offense = new List<string>() { "WANTED BY LSPD", "WANTED FOR ASSAULT", "WANTED FOR UNPAID FINES", "WANTED FOR RUNNING FROM THE POLICE", "WANTED FOR EVADING LAW", "WANTED FOR HIT AND RUN", "WANTED FOR DUI" };
                _offence = $"~r~{Offense[PluginManager.Random.Next(Offense.Count)]}";
                CanBeArrested = true;
                IsWanted = true;
                IsAllowedToBeSearched = true;
            }

            _hasBeenReleased = false;

            Set(Decorators.PED_MISSION, true);
            Set(Decorators.PED_ARRESTABLE, CanBeArrested);

            Create();
        }

        private async void Create()
        {
            this._eventWrapper = new EntityEventWrapper(this.Ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            Decorators.Set(this.Ped.Handle, Decorators.PED_MISSION, false);

            InteractivePed MissionPed = this;
            this.GoToTarget += new InteractivePed.OnGoingToTargetEvent(MissionPed.OnGoToTarget);
            InteractivePed MissionPed1 = this;
            this.AttackTarget += new InteractivePed.OnAttackingTargetEvent(MissionPed1.OnAttackTarget);

            PluginInstance.RegisterEventHandler("curiosity:interaction:idRequesed", new Action<int>(OnIdRequested));
            PluginInstance.RegisterEventHandler("curiosity:interaction:arrest", new Action<int, bool>(OnArrest));

            PluginInstance.RegisterEventHandler("curiosity:interaction:handcuffs", new Action<int, bool>(OnHandcuffs));
            PluginInstance.RegisterEventHandler("curiosity:interaction:cpr", new Action<int, bool>(OnCpr));
            PluginInstance.RegisterEventHandler("curiosity:interaction:cpr:failed", new Action<int>(OnCprFailed));
            PluginInstance.RegisterEventHandler("curiosity:interaction:coroner", new Action<int>(OnCoronerCalled));
            PluginInstance.RegisterEventHandler("curiosity:interaction:searched", new Action<int, bool>(OnPedHasBeenSearched));
            PluginInstance.RegisterEventHandler("curiosity:interaction:leaveAllGroups", new Action<int>(OnPedLeaveGroups));
            PluginInstance.RegisterEventHandler("curiosity:interaction:released", new Action<int>(OnPedHasBeenReleased));
            PluginInstance.RegisterEventHandler("curiosity:interaction:stolencar", new Action<int>(OnStolenCar));
            PluginInstance.RegisterEventHandler("curiosity:interaction:flagRelease", new Action<int>(OnFlagRelease));
            PluginInstance.RegisterEventHandler("curiosity:interaction:grab", new Action<int>(OnGrabPed));
            // group management
            PluginInstance.RegisterEventHandler("curiosity:setting:group:join", new Action<int>(OnGroupJoin));
            PluginInstance.RegisterEventHandler("curiosity:setting:group:leave", new Action<int>(OnGroupLeave));
            // car stolen
            PluginInstance.RegisterEventHandler("curiosity:interaction:hasLostId", new Action<int>(OnHasLostId));

            PluginInstance.RegisterTickHandler(OnShowHelpTextTask);

            if (PlayerManager.IsDeveloper)
                PluginInstance.RegisterTickHandler(OnShowDeveloperOverlayTask);

            if (Ped.AttachedBlip == null)
            {
                Ped.AttachBlip();
                Ped.AttachedBlip.Color = BlipColor.TrevorOrange;
                Ped.AttachedBlip.Sprite = BlipSprite.Standard;
                Ped.AttachedBlip.Scale = 0.5f;
            }

            if (IsUnderTheInfluence)
            {
                if (!HasAnimSetLoaded(MOVEMENT_ANIMATION_SET_DRUNK))
                {
                    RequestAnimSet(MOVEMENT_ANIMATION_SET_DRUNK);
                    while (!HasAnimSetLoaded(MOVEMENT_ANIMATION_SET_DRUNK))
                    {
                        await PluginManager.Delay(0);
                    }
                }
                this.Ped.MovementAnimationSet = MOVEMENT_ANIMATION_SET_DRUNK;
            }

            Ped.RelationshipGroup = Relationships.NeutralRelationship;

            if (!this.Ped.IsInVehicle() && !this.Ped.IsInGroup) // Ignore this if they are in a vehicle
            {
                if (_chanceOfShootAndFlee == 4)
                {
                    WeaponHash weaponHash = WeaponHash.Pistol;
                    if (PluginManager.Random.Next(5) == 4)
                    {
                        weaponHash = WeaponHash.SawnOffShotgun;
                    }
                    this.Ped.Weapons.Give(weaponHash, 1, true, true);
                    this.Ped.Task.ShootAt(Game.PlayerPed);
                    await PluginManager.Delay(3000);
                    this.Ped.Task.FleeFrom(Game.PlayerPed);

                    Ped.RelationshipGroup = Relationships.HostileRelationship;

                    if (Ped.AttachedBlip != null)
                        Ped.AttachedBlip.Color = BlipColor.Red;
                }
                else if (_chanceOfFlee >= 28)
                {
                    this.Ped.Task.FleeFrom(Game.PlayerPed);

                    Ped.RelationshipGroup = Relationships.DislikeRelationship;

                    if (Ped.AttachedBlip != null)
                        Ped.AttachedBlip.Color = BlipColor.Red;
                }
            }

            int networkId = API.NetworkGetNetworkIdFromEntity(base.Handle);

            NpcHandler.AddNpc(base.Handle, this);
            if (Ped.IsInVehicle())
            {
                DecorSetInt(Ped.Handle, PluginManager.DECOR_NPC_CURRENT_VEHICLE, Ped.CurrentVehicle.Handle);
            }
        }

        public abstract void OnGoToTarget(Ped target);
        public abstract void OnAttackTarget(Ped target);

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
            if (!string.IsNullOrEmpty(helpText))
                Screen.DisplayHelpTextThisFrame(helpText);
        }

        private async void OnDied(EntityEventWrapper sender, Entity entity)
        {
            if (base.IsOccluded)
            {
                base.Delete();
            }

            await PluginManager.Delay(3000);
            API.NetworkFadeOutEntity(this.Handle, false, false);
            await PluginManager.Delay(1000);
            sender.Dispose();
        }

        private bool IsGoodTarget(Ped ped)
        {
            return ped.GetRelationshipWithPed(this.Ped) == Relationship.Hate;
        }

        private static bool IsBehind(float distance)
        {
            return distance < InteractivePed.BehindNoticeDistance;
        }

        private static bool IsRunningNoticed(Ped ped, float distance)
        {
            return (!ped.IsSprinting ? false : distance < InteractivePed.RunningNoticeDistance);
        }

        private static bool IsWeaponWellSilenced(Ped ped, float distance)
        {
            bool flag;
            if (ped.IsShooting)
            {
                flag = (!ped.IsCurrentWeaponSileced() ? false : distance > InteractivePed.SilencerEffectiveRange);
            }
            else
            {
                flag = true;
            }
            return flag;
        }

        private bool CanHearPed(Ped ped)
        {
            float single = ped.Position.VDist(this.Position);
            return (!InteractivePed.IsWeaponWellSilenced(ped, single) || InteractivePed.IsBehind(single) ? true : InteractivePed.IsRunningNoticed(ped, single));
        }

        private void GetTarget()
        {
            bool flag;
            Ped[] array = World.GetAllPeds().Where<Ped>(new Func<Ped, bool>(this.IsGoodTarget)).ToArray<Ped>();
            Ped closest = World.GetClosest<Ped>(this.Position, array);
            if (closest == null)
            {
                flag = false;
            }
            else
            {
                flag = (this.Ped.HasClearLineOfSight(closest, InteractivePed.VisionDistance) ? true : this.CanHearPed(closest));
            }
            if (flag)
            {
                this.Target = closest;
            }
            else if ((!(this.Target != null) || this.IsGoodTarget(this.Target) ? closest != this.Target : true))
            {
                this.Target = null;
            }
        }

        public async void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            try
            {
                int networkId = API.NetworkGetNetworkIdFromEntity(base.Handle);

                bool flag;
                if (this.Position.VDist(Game.PlayerPed.Position) <= 120f)
                {
                    flag = false;
                }
                else
                {
                    flag = (!base.IsOnScreen ? true : base.IsDead);
                }
                if (flag)
                {
                    Scripts.NpcHandler.RemoveNpc(base.Handle);
                    base.Delete();

                    PluginInstance.DeregisterTickHandler(OnShowHelpTextTask);

                    if (PlayerManager.IsDeveloper)
                        PluginInstance.DeregisterTickHandler(OnShowDeveloperOverlayTask);
                }

                if (this.Ped == null)
                {
                    Scripts.NpcHandler.RemoveNpc(base.Handle);
                    base.Delete();

                    PluginInstance.DeregisterTickHandler(OnShowHelpTextTask);

                    if (PlayerManager.IsDeveloper)
                        PluginInstance.DeregisterTickHandler(OnShowDeveloperOverlayTask);

                    return;
                }

                this.GetTarget();

                if (this.Target != null)
                {
                    if (this.Position.VDist(this.Target.Position) <= InteractivePed.AttackRange)
                    {
                        this.AttackingTarget = true;
                        this.GoingToTarget = false;
                    }
                    else
                    {
                        this.AttackingTarget = false;
                        this.GoingToTarget = true;
                    }
                }

                if (_hasBeenReleased)
                {
                    Scripts.NpcHandler.RemoveNpc(base.Handle);

                    Ped.RelationshipGroup = Relationships.NeutralRelationship;

                    OnPedLeaveGroups(Ped.Handle);
                    OnPedHasBeenReleased(Ped.Handle);

                    if (Ped.IsInGroup)
                        Ped.PedGroup.Delete();

                    Ped.LeaveGroup();

                    await PluginManager.Delay(100);

                    Ped.Task.WanderAround();

                    if (Ped.IsOccluded)
                    {
                        base.Delete();
                    }

                    return;
                }

                if (this.Ped.IsBeingStunned)
                {
                    this.Ped.Health = 200;

                    if (PluginManager.Random.Next(5) >= 3 && !IsArrested)
                    {
                        IsArrested = true;
                        ArrestInteractions.InteractionArrestInit(this);
                    }
                }

                if (this.Ped.IsInjured)
                {
                    if (PluginManager.Random.Next(5) >= 3 && !IsArrested)
                    {
                        if (PluginManager.Random.Next(30) == 28)
                        {
                            this.Ped.SetConfigFlag(187, true);
                        }
                        else
                        {
                            this.Ped.SetConfigFlag(166, true);
                            IsArrested = true;
                            ArrestInteractions.InteractionArrestInit(this);
                        }
                    }
                }

                if (this.Ped.IsOccluded)
                {
                    if (Ped != null)
                    {
                        if (Ped.AttachedBlip != null)
                        {
                            if (Ped.AttachedBlip.Alpha == 255)
                                Ped.AttachedBlip.Alpha = 0;
                        }
                    }
                }
                else
                {
                    if (Ped != null)
                    {
                        if (Ped.AttachedBlip != null)
                        {
                            if (Ped.AttachedBlip.Alpha == 0)
                                Ped.AttachedBlip.Alpha = 255;
                        }
                    }
                }

                if (IsArrested)
                {
                    if (this.Ped.Position.Distance(Game.PlayerPed.Position) >= 20f && !this.Ped.IsInVehicle())
                    {
                        IsArrested = false;
                        this.Ped.LeaveGroup();

                        this.Ped.Task.FleeFrom(Game.PlayerPed);

                        if (Ped.AttachedBlip != null)
                            Ped.AttachedBlip.Color = BlipColor.Red;
                    }
                }

                if (Ped.Position.Distance(Game.PlayerPed.Position) >= 200f)
                {
                    Scripts.NpcHandler.RemoveNpc(base.Handle);
                    OnPedHasBeenReleased(Ped.Handle);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }

        public void SetRelationship(RelationshipGroup relationshipGroup)
        {
            this.Ped.RelationshipGroup = relationshipGroup;
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();

            PluginInstance.DeregisterTickHandler(OnShowHelpTextTask);

            //  PluginInstance.TriggerEvent("curiosity:Client:Missions:RandomEventCompleted");

            if (PlayerManager.IsDeveloper)
                PluginInstance.DeregisterTickHandler(OnShowDeveloperOverlayTask);
        }





        private void OnCpr(int handle, bool state)
        {
            if (Handle == handle)
                IsPerformingCpr = state;
        }

        private void OnCprFailed(int handle)
        {
            if (Handle == handle)
            {
                IsPerformingCpr = false;
                HasCprFailed = true;
            }
        }

        private void OnCoronerCalled(int handle)
        {
            if (Handle == handle)
            {
                Ped.Detach();
                HasBeenGrabbed = false;

                IsCoronerCalled = true;
                // PluginInstance.TriggerEvent("curiosity:Client:Missions:RandomEventCompleted");
            }
        }

        public void OnIdRequested(int handle)
        {
            if (Handle == handle)
                HasProvidedId = true;
        }

        public void OnPedHasBeenSearched(int handle, bool illegalItems)
        {
            if (Handle == handle)
            {
                HasBeenSearched = true;
                IsCarryingIllegalItems = illegalItems;
                CanBeArrested = illegalItems;
            }
        }

        public void OnHandcuffs(int handle, bool state)
        {
            if (Ped.Handle == handle)
            {
                SoundManager.PlaySFX($"CUFFS_TIGHTEN_01");

                API.SetPedFleeAttributes(handle, 0, false);
                API.SetBlockingOfNonTemporaryEvents(handle, true);
                API.TaskSetBlockingOfNonTemporaryEvents(handle, true);

                IsHandcuffed = state;
                IsArrested = state;
                DecorSetBool(handle, PluginManager.DECOR_NPC_ARRESTED, state);

                if (!IsArrested)
                {
                    if (Ped.IsInGroup)
                        Ped.LeaveGroup();
                }
            }
        }

        public void OnHasLostId(int handle)
        {
            if (HasProvidedId) return;

            if (Handle == handle)
            {
                if (PluginManager.Random.Next(80, 100) >= 95)
                {
                    HasLostId = true;
                    IsAllowedToBeSearched = true;
                }
            }
        }

        public void OnIdRan(int handle)
        {
            if (Handle == handle)
            {
                HasIdentifcationBeenRan = true;
            }
        }

        public void OnStolenCar(int handle)
        {
            if (Handle == handle)
            {
                _stolenCar = true;
            }
        }

        public void OnGroupJoin(int handle)
        {
            if (Handle == handle)
            {
                int playerGroupId = API.GetPedGroupIndex(Game.PlayerPed.Handle);

                if (playerGroupId < 0)
                {
                    playerGroupId = API.CreateGroup(0);
                    SetPedAsGroupMember(playerGroupId, Game.PlayerPed.Handle);
                    SetPedAsGroupLeader(Game.PlayerPed.Handle, playerGroupId);
                }

                SetPedAsGroupMember(handle, playerGroupId);
                SetPedCanTeleportToGroupLeader(handle, playerGroupId, true);
            }
        }

        public async void OnGroupLeave(int handle)
        {
            await BaseScript.Delay(0);
            if (Handle == handle)
            {
                Ped.Task.ClearAll();
                Ped.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();

                Ped.LeaveGroup();
            }
        }

        public void OnFlagRelease(int handle)
        {
            if (Handle == handle)
            {
                _hasBeenReleased = true;
                // PluginInstance.TriggerEvent("curiosity:Client:Missions:RandomEventCompleted");
            }
        }

        async void OnPedLeaveGroups(int handle)
        {
            await BaseScript.Delay(100);

            if (PluginManager.DeveloperNpcUiEnabled)
            {
                Screen.ShowNotification("~b~NPC: ~w~OnPedLeaveGroups");
                Screen.ShowNotification($"~b~NPC: ~w~{Handle}\nEvent: {handle}");
            }

            if (Handle == handle)
            {
                Ped.Task.ClearAll();
                Ped.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();

                Ped.NeverLeavesGroup = false;
                Ped.LeaveGroup();
                API.RemovePedFromGroup(Handle);

                if (Ped.IsInGroup)
                    Ped.PedGroup.Delete();

                if (PluginManager.DeveloperNpcUiEnabled)
                {
                    Screen.ShowNotification("~b~NPC: ~w~OnPedLeaveGroups: ~r~Success");
                }
            }
        }

        private async void OnPedHasBeenReleased(int handle)
        {
            if (Handle == handle)
            {

                Ped.Detach();
                HasBeenGrabbed = false;

                _hasBeenReleased = true;

                Scripts.NpcHandler.RemoveNpc(base.Handle);

                DecorSetBool(handle, PluginManager.DECOR_NPC_ARRESTED, false);

                Ped.SetConfigFlag(292, false);
                Ped.SetConfigFlag(301, false);

                if (Ped.IsInVehicle() && Ped.CurrentVehicle == PluginManager.CurrentVehicle)
                {
                    Ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                }

                if (Ped.CurrentVehicle != null)
                {
                    Ped.CurrentVehicle.IsPositionFrozen = false;

                    if (Ped.CurrentVehicle.AttachedBlip != null)
                    {
                        if (Ped.CurrentVehicle.AttachedBlip.Exists())
                            Ped.CurrentVehicle.AttachedBlip.Delete();
                    }
                }

                if (Ped.AttachedBlip != null)
                {
                    if (Ped.AttachedBlip.Exists())
                        Ped.AttachedBlip.Delete();
                }

                Ped.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();

                Ped.Task.ClearAll();
                Ped.MarkAsNoLongerNeeded();
                Ped.IsPersistent = false;
                Ped.LeaveGroup();

                API.SetPedFleeAttributes(Handle, 0, true);
                API.SetBlockingOfNonTemporaryEvents(Handle, false);

                API.TaskSetBlockingOfNonTemporaryEvents(Ped.Handle, false);

                DecorSetBool(Handle, PluginManager.DECOR_NPC_WAS_RELEASED, _hasBeenReleased);

                PluginInstance.DeregisterTickHandler(OnShowHelpTextTask);

                if (PlayerManager.IsDeveloper)
                    PluginInstance.DeregisterTickHandler(OnShowDeveloperOverlayTask);

                if (DecorExistOn(Handle, PluginManager.DECOR_NPC_CURRENT_VEHICLE))
                {
                    int vehId = DecorGetInt(Handle, PluginManager.DECOR_NPC_CURRENT_VEHICLE);
                    if (DoesEntityExist(vehId))
                    {
                        Vehicle vehicle = new Vehicle(vehId);
                        Ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver, 5000, 5f);
                        await PluginManager.Delay(5000);
                        Ped.Task.WanderAround(Ped.Position, 1000f);

                        if (vehicle.AttachedBlip != null)
                        {
                            if (vehicle.AttachedBlip.Exists())
                                vehicle.AttachedBlip.Delete();
                        }

                        if (Decorators.GetBoolean(vehicle.Handle, PluginManager.DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED))
                        {
                            BaseScript.TriggerEvent("curiosity:interaction:vehicle:released", vehicle.Handle);
                        }
                    }
                }
                else
                {
                    Ped.Task.WanderAround(Ped.Position, 1000f);

                    if (Ped.IsInVehicle())
                    {
                        if (Decorators.GetBoolean(Ped.CurrentVehicle.Handle, PluginManager.DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED))
                        {
                            BaseScript.TriggerEvent("curiosity:interaction:vehicle:released", Ped.CurrentVehicle.Handle);
                        }
                    }
                }

                // PluginInstance.TriggerEvent("curiosity:Client:Missions:RandomEventCompleted");
            }
        }

        public event InteractivePed.OnAttackingTargetEvent AttackTarget;
        public delegate void OnAttackingTargetEvent(Ped target);
        public event InteractivePed.OnGoingToTargetEvent GoToTarget;
        public delegate void OnGoingToTargetEvent(Ped target);

        void OnGrabPed(int handle)
        {
            if (handle != Handle) return;

            if (HasBeenGrabbed)
            {
                Ped.Detach();
                HasBeenGrabbed = false;
            }
            else if (!HasBeenGrabbed)
            {
                Vector3 attachedPos = new Vector3(-0.3f, 0.4f, 0.0f);
                Ped.AttachTo(Game.PlayerPed, attachedPos);
                SetBlockingOfNonTemporaryEvents(handle, true);
                HasBeenGrabbed = true;
            }
        }

        private async void OnArrest(int handle, bool interaction = false)
        {
            if (Handle != handle)
            {
                base.Delete();

                PluginInstance.DeregisterTickHandler(OnShowHelpTextTask);

                if (PlayerManager.IsDeveloper)
                    PluginInstance.DeregisterTickHandler(OnShowDeveloperOverlayTask);

                return;
            }


            if (!IsHandcuffed)
            {
                List<string> insults = new List<string>()
                {
                    "idiot", "fool", "muppet", "pleb", "maguire"
                };

                string randomInsult = insults[PluginManager.Random.Next(insults.Count + 1)];

                List<string> vs = new List<string> { $"~o~WHY AREN'T THEY CUFFED!", $"~o~Handcuff them you {randomInsult}!!", "~r~WHAT IS YOUR MAJOR MALFUNCTION! PUT ON THE CUFFS!!!", $"~r~Cuff them, fecking {randomInsult}!" };
                Screen.ShowNotification(vs[PluginManager.Random.Next(vs.Count + 1)]);
                return;
            }

            ArrestedPedData arrestedPedData = new ArrestedPedData();

            arrestedPedData.IsDrunk = IsUnderTheInfluence;
            arrestedPedData.IsDrugged = IsUsingCocaine || IsUsingCannabis;
            arrestedPedData.IsDrivingStolenCar = HasStolenCar;
            arrestedPedData.IsCarryingIllegalItems = IsCarryingIllegalItems || IsCarryingStolenItems;
            arrestedPedData.CaughtSpeeding = CaughtSpeeding;
            arrestedPedData.IsWanted = IsWanted;

            arrestedPedData.DispatchJail = interaction;

            if (PluginManager.CurrentVehicle != null)
                arrestedPedData.IsBike = PluginManager.CurrentVehicle.Model.IsBike;

            arrestedPedData.IsAllowedToBeArrested = (
                arrestedPedData.IsDrunk
                || arrestedPedData.IsDrugged
                || arrestedPedData.IsDrivingStolenCar
                || arrestedPedData.IsCarryingIllegalItems
                || arrestedPedData.CaughtSpeeding
                || arrestedPedData.IsWanted
                || CanBeArrested
                || RanFromPolice);

            string encoded = Encode.StringToBase64(JsonConvert.SerializeObject(arrestedPedData));

            PluginManager.TriggerServerEvent("curiosity:Server:Missions:ArrestedPed", encoded);

            if (CanBeArrested)
            {
                Screen.ShowNotification($"~g~They've been booked.");
            }

            Ped.IsPositionFrozen = false;
            if (Ped.IsInVehicle())
            {
                Ped.SetConfigFlag(292, false);
                Ped.Task.LeaveVehicle();
            }
            API.NetworkFadeOutEntity(Handle, true, false);
            _hasBeenReleased = true;
            await PluginManager.Delay(500);

            Ped.Detach();
            HasBeenGrabbed = false;

            NpcHandler.RemoveNpc(base.Handle);

            base.Delete();
        }

        private async Task OnShowDeveloperOverlayTask()
        {
            await Task.FromResult(0);

            if (!PluginManager.DeveloperNpcUiEnabled)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (Position.Distance(Game.PlayerPed.Position) >= 6) return;

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            keyValuePairs.Add("Name", Name);
            keyValuePairs.Add("DoB", DateOfBirth);
            keyValuePairs.Add("Health", $"{Health} / {MaxHealth}");
            keyValuePairs.Add("-", "");
            keyValuePairs.Add("_ChanceOfFlee", $"{_chanceOfFlee}");
            keyValuePairs.Add("_ChanceOfShootAndFlee", $"{_chanceOfShootAndFlee}");
            keyValuePairs.Add("--", "");
            keyValuePairs.Add("IsArrested", $"{IsArrested}");
            keyValuePairs.Add("IsHandcuffed", $"{IsHandcuffed}");
            keyValuePairs.Add("HasProvidedId", $"{HasProvidedId}");
            keyValuePairs.Add("HasLostId", $"{HasLostId}");
            keyValuePairs.Add("HasBeenSearched", $"{HasBeenSearched}");
            keyValuePairs.Add("---", "");
            keyValuePairs.Add("CanBeArrested", $"{CanBeArrested}");
            keyValuePairs.Add("IsAllowedToBeSearched", $"{IsAllowedToBeSearched}");
            keyValuePairs.Add("IsUsingCannabis", $"{IsUsingCannabis}");
            keyValuePairs.Add("IsUsingCocaine", $"{IsUsingCocaine}");
            keyValuePairs.Add("IsCarryingIllegalItems", $"{IsCarryingIllegalItems}");
            keyValuePairs.Add("IsUnderTheInfluence", $"{IsUnderTheInfluence}");
            keyValuePairs.Add("IsInGroup", $"{Ped.IsInGroup}");

            if (Ped.IsInGroup)
                keyValuePairs.Add("Group", $"{Ped.RelationshipGroup}");

            keyValuePairs.Add("----", "");
            keyValuePairs.Add("BloodAlcaholLimit", $"{BloodAlcaholLimit}");
            keyValuePairs.Add("Attitude", $"{Attitude}");
            keyValuePairs.Add("NumberOfCitations", $"{NumberOfCitations}");
            keyValuePairs.Add("Offence", $"{Offence}");

            if (Vehicle != null)
            {
                keyValuePairs.Add("-----", "");
                keyValuePairs.Add("NPC_CURRENT_VEHICLE", $"{DecorGetInt(Ped.Handle, PluginManager.DECOR_NPC_CURRENT_VEHICLE)}");
                keyValuePairs.Add("Local Vehicle", $"{Vehicle.Handle}");
            }

            Wrappers.Helpers.DrawData(this, keyValuePairs);
        }

        public void Set(string property, object value)
        {
            if (value is int i)
            {
                if (!API.DecorExistOn(Handle, property))
                {
                    API.DecorRegister(property, 3);
                }

                API.DecorSetInt(Handle, property, i);
            }
            else if (value is float f)
            {
                if (!API.DecorExistOn(Handle, property))
                {
                    API.DecorRegister(property, 1);
                }

                API.DecorSetFloat(Handle, property, f);
            }
            else if (value is bool b)
            {
                if (!API.DecorExistOn(Handle, property))
                {
                    API.DecorRegister(property, 2);
                }

                API.DecorSetBool(Handle, property, b);
            }
            else
            {
                Log.Info("[Decor] Could not set decor object due to it not being a supported type.");
            }
        }

        public int GetInteger(string property)
        {
            return API.DecorExistOn(Handle, property) ? API.DecorGetInt(Handle, property) : 0;
        }

        public float GetFloat(string property)
        {
            return API.DecorExistOn(Handle, property) ? API.DecorGetFloat(Handle, property) : 0f;
        }

        public bool GetBoolean(string property)
        {
            return API.DecorExistOn(Handle, property) && API.DecorGetBool(Handle, property);
        }
    }
}
