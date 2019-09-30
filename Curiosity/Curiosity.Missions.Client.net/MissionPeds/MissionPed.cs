using CitizenFX.Core;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class MissionPed : Entity, IEquatable<Ped>
    {
        public const int MovementUpdateInterval = 5;

        public static int Damage;

        public static float SensingRange;

        public static float SilencerEffectiveRange;

        public static float BehindNoticeDistance;

        public static float RunningNoticeDistance;

        public static float AttackRange;

        public static float VisionDistance;

        public static float WanderRadius;

        private Ped _target;

        private readonly Ped _ped;

        private EntityEventWrapper _eventWrapper;

        private bool _goingToTarget;

        private bool _attackingTarget;

        public List<Vector3> Waypoints;

        private int NextWaypoint = 0;

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
                    MissionPed.OnAttackingTargetEvent onAttackingTargetEvent = this.AttackTarget;
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
                    MissionPed.OnGoingToTargetEvent onGoingToTargetEvent = this.GoToTarget;
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

        static MissionPed()
        {
            MissionPed.Damage = 15;
            MissionPed.SensingRange = 120f;
            MissionPed.SilencerEffectiveRange = 15f;
            MissionPed.BehindNoticeDistance = 5f;
            MissionPed.RunningNoticeDistance = 25f;
            MissionPed.AttackRange = 30f;
            MissionPed.VisionDistance = 35f;
            MissionPed.WanderRadius = 100f;
        }

        protected MissionPed(int handle) : base(handle)
        {
            this._ped = new Ped(handle);
            this._eventWrapper = new EntityEventWrapper(this._ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            MissionPed MissionPed = this;
            this.GoToTarget += new MissionPed.OnGoingToTargetEvent(MissionPed.OnGoToTarget);
            MissionPed MissionPed1 = this;
            this.AttackTarget += new MissionPed.OnAttackingTargetEvent(MissionPed1.OnAttackTarget);
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();
        }

        private bool CanHearPed(Ped ped)
        {
            float single = ped.Position.VDist(this.Position);
            return (!MissionPed.IsWeaponWellSilenced(ped, single) || MissionPed.IsBehindZombie(single) ? true : MissionPed.IsRunningNoticed(ped, single));
        }

        protected bool Equals(MissionPed other)
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
                flag = (obj.GetType() != base.GetType() ? false : this.Equals((MissionPed)obj));
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
                flag = (this._ped.HasClearLineOfSight(closest, MissionPed.VisionDistance) ? true : this.CanHearPed(closest));
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

        private static bool IsBehindZombie(float distance)
        {
            return distance < MissionPed.BehindNoticeDistance;
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
                flag = (!ped.IsCurrentWeaponSileced() ? false : distance > MissionPed.SilencerEffectiveRange);
            }
            else
            {
                flag = true;
            }
            return flag;
        }

        public abstract void OnAttackTarget(Ped target);

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            Entity killerEnt = new Ped(entity.Handle).GetKiller();
            Ped killerPed = new Ped(killerEnt.Handle);

            if (killerPed.IsPlayer)
            {
                CitizenFX.Core.UI.Screen.ShowNotification($"Mission Ped {entity.Handle}");
            }

            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
        }

        public abstract void OnGoToTarget(Ped target);

        public static implicit operator Ped(MissionPed v)
        {
            return v._ped;
        }

        private void GotoWaypoint()
        {
            if (this.Target != null)
            {
                
            }
            else
            {
                if (Waypoints != null)
                {
                    if (Waypoints.Count > 0)
                    {

                        if (NextWaypoint > Waypoints.Count - 1)
                        {
                            NextWaypoint = 0;
                        }

                        Vector3 wp = Waypoints[NextWaypoint];
                        this._ped.Task.GoTo(wp);

                        if (wp.DistanceToSquared(this._ped.Position) < 2f)
                        {
                            NextWaypoint++;
                        }
                    }
                }
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

            this.GetTarget();

            if (Waypoints != null)
                this.GotoWaypoint();

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

        public event MissionPed.OnAttackingTargetEvent AttackTarget;

        public event MissionPed.OnGoingToTargetEvent GoToTarget;

        public delegate void OnAttackingTargetEvent(Ped target);

        public delegate void OnGoingToTargetEvent(Ped target);
    }
}
