using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using System;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class ZombiePed : Entity, IEquatable<Ped>
    {
        public const int MovementUpdateInterval = 5;

        public static int ZombieDamage;

        public static float SensingRange;

        public static float SilencerEffectiveRange;

        public static float BehindZombieNoticeDistance;

        public static float RunningNoticeDistance;

        public static float AttackRange;

        public static float VisionDistance;

        public static float WanderRadius;

        private Ped _target;

        private readonly Ped _ped;

        private EntityEventWrapper _eventWrapper;

        private bool _goingToTarget;

        private bool _attackingTarget;

        private DateTime _currentMovementUpdateTime;

        public bool AttackingTarget
        {
            get
            {
                return this._attackingTarget;
            }
            set
            {
                if ((!value || this._ped.IsRagdoll || base.IsDead || this._ped.IsClimbing || this._ped.IsFalling || this._ped.IsBeingStunned ? false : !this._ped.IsGettingUp))
                {
                    ZombiePed.OnAttackingTargetEvent onAttackingTargetEvent = this.AttackTarget;
                    if (onAttackingTargetEvent != null)
                    {
                        onAttackingTargetEvent(this.Target);
                    }
                    else
                    {
                    }
                }
                this._attackingTarget = value;
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
                    ZombiePed.OnGoingToTargetEvent onGoingToTargetEvent = this.GoToTarget;
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

        public virtual string MovementStyle
        {
            get;
            set;
        }

        public virtual bool PlayAudio
        {
            get;
            set;
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
                    this._ped.Task.WanderAround(this.Position, MissionPed.WanderRadius);
                    int num = 0;
                    bool flag = num == 1;
                    this.AttackingTarget = num == 1;
                    this.GoingToTarget = flag;
                }
                this._target = value;
            }
        }

        static ZombiePed()
        {
            ZombiePed.ZombieDamage = 15;
            ZombiePed.SensingRange = 120f;
            ZombiePed.SilencerEffectiveRange = 15f;
            ZombiePed.BehindZombieNoticeDistance = 5f;
            ZombiePed.RunningNoticeDistance = 25f;
            ZombiePed.AttackRange = 1.2f;
            ZombiePed.VisionDistance = 35f;
            ZombiePed.WanderRadius = 100f;
        }

        protected ZombiePed(int handle) : base(handle)
        {
            this._ped = new Ped(handle);
            this._eventWrapper = new EntityEventWrapper(this._ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);
            this._currentMovementUpdateTime = DateTime.UtcNow;
            ZombiePed zombiePed = this;
            this.GoToTarget += new ZombiePed.OnGoingToTargetEvent(zombiePed.OnGoToTarget);
            ZombiePed zombiePed1 = this;
            this.AttackTarget += new ZombiePed.OnAttackingTargetEvent(zombiePed1.OnAttackTarget);

            NetworkRequestControlOfEntity(this._ped.Handle);
            int networkId = API.NetworkGetNetworkIdFromEntity(this._ped.Handle);
            SetNetworkIdCanMigrate(networkId, true);
            NetworkRegisterEntityAsNetworked(networkId);
            SetNetworkIdExistsOnAllMachines(networkId, true);

            if (!IsEntityAMissionEntity(this._ped.Handle))
                SetEntityAsMissionEntity(this._ped.Handle, true, true);
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();
        }

        private bool CanHearPed(Ped ped)
        {
            float single = ped.Position.VDist(this.Position);
            return (!ZombiePed.IsWeaponWellSilenced(ped, single) || ZombiePed.IsBehindZombie(single) ? true : ZombiePed.IsRunningNoticed(ped, single));
        }

        protected bool Equals(ZombiePed other)
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
            else if (this != obj)
            {
                flag = (obj.GetType() != base.GetType() ? false : this.Equals((ZombiePed)obj));
            }
            else
            {
                flag = true;
            }
            return flag;
        }

        public bool Equals(Ped other)
        {
            return object.Equals(this._ped, other);
        }

        public void ForgetTarget()
        {
            this._target = null;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this._ped != null ? this._ped.GetHashCode() : 0);
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
                flag = (this._ped.HasClearLineOfSight(closest, ZombiePed.VisionDistance) ? true : this.CanHearPed(closest));
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

        public void InfectTarget(Ped target)
        {
            if (!target.IsPlayer)
            {
                if (target.Health <= target.MaxHealth / 4)
                {
                    target.SetToRagdoll(3000);
                    Scripts.PedCreators.ZombieCreator.InfectPed(target, this.MaxHealth, true);
                    this.ForgetTarget();
                    target.LeaveGroup();
                    target.Weapons.RemoveAll();
                    EntityEventWrapper.Dispose(target);
                }
            }
        }

        private static bool IsBehindZombie(float distance)
        {
            return distance < ZombiePed.BehindZombieNoticeDistance;
        }

        private bool IsGoodTarget(Ped ped)
        {
            return ped.GetRelationshipWithPed(this._ped) == Relationship.Hate;
        }

        private static bool IsRunningNoticed(Ped ped, float distance)
        {
            return (!ped.IsSprinting ? false : distance < MissionPed.RunningNoticeDistance);
        }

        private static bool IsWeaponWellSilenced(Ped ped, float distance)
        {
            bool flag;
            if (ped.IsShooting)
            {
                flag = (!ped.IsCurrentWeaponSileced() ? false : distance > ZombiePed.SilencerEffectiveRange);
            }
            else
            {
                flag = true;
            }
            return flag;
        }

        public abstract void OnAttackTarget(Ped target);

        private async void OnDied(EntityEventWrapper sender, Entity entity)
        {
            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }

            await Client.Delay(3000);
            API.NetworkFadeOutEntity(this.Handle, false, false);
            await Client.Delay(1000);
            

            // CitizenFX.Core.UI.Screen.ShowNotification("Killed Zombie");

            //if ((!ZombieVehicleSpawner.Instance.IsInvalidZone(entity.get_Position()) ? false : ZombieVehicleSpawner.Instance.IsValidSpawn(entity.get_Position())))
            //{
            //    ZombieVehicleSpawner.Instance.SpawnBlocker.Add(entity.get_Position());
            //}
        }

        public abstract void OnGoToTarget(Ped target);

        public static implicit operator Ped(ZombiePed v)
        {
            return v._ped;
        }

        private void SetWalkStyle()
        {
            if (DateTime.UtcNow > this._currentMovementUpdateTime)
            {
                this._ped.SetMovementAnimSet(this.MovementStyle);
                this.UpdateTime();
            }
        }

        public void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
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
                base.Delete();
            }
            if ((!this.PlayAudio ? false : this._ped.IsRunning))
            {
                this._ped.DisablePainAudio(false);
                this._ped.PlayPain(8);
                this._ped.PlayFacialAnim("facials@gen_male@base", "burning_1");
            }
            this.GetTarget();
            this.SetWalkStyle();
            if ((!this._ped.IsOnFire ? false : !this._ped.IsDead))
            {
                this._ped.Kill();
            }
            this._ped.StopAmbientSpeechThisFrame();
            if (!this.PlayAudio)
            {
                this._ped.StopSpeaking(true);
            }
            if (this.Target != null)
            {
                if (this.Position.VDist(this.Target.Position) <= MissionPed.AttackRange)
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
        }

        private void UpdateTime()
        {
            this._currentMovementUpdateTime = DateTime.UtcNow + new TimeSpan(0, 0, 0, 5);
        }

        public event ZombiePed.OnAttackingTargetEvent AttackTarget;

        public event ZombiePed.OnGoingToTargetEvent GoToTarget;

        public delegate void OnAttackingTargetEvent(Ped target);

        public delegate void OnGoingToTargetEvent(Ped target);
    }
}
