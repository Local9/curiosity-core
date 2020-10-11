using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Missions.Client.net.Classes.PlayerClient;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

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

        public static bool IsHostage = false;

        private Ped _target;

        private readonly Ped _ped;

        private EntityEventWrapper _eventWrapper;

        private bool _goingToTarget;

        private bool _attackingTarget;

        public List<Vector3> Waypoints;

        private int NextWaypoint = 0;

        public float Experience;

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
            MissionPed.WanderRadius = 10f;
        }

        protected MissionPed(int handle, float visionDistance = 100f, float experience = 10f, bool isHostage = false) : base(handle)
        {
            this._ped = new Ped(handle);
            this._eventWrapper = new EntityEventWrapper(this._ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            // 1/10 chance of armor
            if (Client.Random.Next(10) == 9)
                this._ped.Armor = Client.Random.Next(100);

            Decorators.Set(this._ped.Handle, Client.DECOR_PED_MISSION, true);
            Decorators.Set(this._ped.Handle, Client.DECOR_PED_HOSTAGE, isHostage);

            VisionDistance = visionDistance;
            AttackRange = visionDistance;
            Experience = experience;

            MissionPed MissionPed = this;
            this.GoToTarget += new MissionPed.OnGoingToTargetEvent(MissionPed.OnGoToTarget);
            MissionPed MissionPed1 = this;
            this.AttackTarget += new MissionPed.OnAttackingTargetEvent(MissionPed1.OnAttackTarget);

            NetworkRequestControlOfEntity(this._ped.Handle);
            int networkId = API.NetworkGetNetworkIdFromEntity(this._ped.Handle);
            SetNetworkIdCanMigrate(networkId, true);
            NetworkRegisterEntityAsNetworked(networkId);
            SetNetworkIdExistsOnAllMachines(networkId, true);

            if (!IsEntityAMissionEntity(this._ped.Handle))
                SetEntityAsMissionEntity(this._ped.Handle, true, true);

            Client.GetInstance().RegisterTickHandler(OnDevUI);
        }

        private async Task OnDevUI()
        {
            if (Decorators.GetBoolean(Game.PlayerPed.Handle, "player::npc::debug"))
            {
                if (Position.Distance(Game.PlayerPed.Position) >= 6) return;

                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                keyValuePairs.Add("Attacking Target", $"{this.AttackingTarget}");
                keyValuePairs.Add("Going to Target", $"{this.GoingToTarget}");
                keyValuePairs.Add("Has Target", $"{this.Target != null}");
                keyValuePairs.Add("---", $"---");
                keyValuePairs.Add("Target", $"{this.Target}");

                Wrappers.Helpers.DrawData(this, keyValuePairs);
            }
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();
            Client.GetInstance().DeregisterTickHandler(OnDevUI);
        }

        private bool CanHearPed(Ped ped)
        {
            float single = ped.Position.VDist(this.Position);
            return (!MissionPed.IsWeaponWellSilenced(ped, single) || MissionPed.IsBehind(single) ? true : MissionPed.IsRunningNoticed(ped, single));
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

            Ped[] playerArray = World.GetAllPeds().Where(x => x.IsPlayer).ToArray<Ped>();

            Ped[] array = playerArray.Where<Ped>(new Func<Ped, bool>(this.IsGoodTarget)).ToArray<Ped>();

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

        private static bool IsBehind(float distance)
        {
            return distance < MissionPed.BehindNoticeDistance;
        }

        private bool IsGoodTarget(Ped ped)
        {
            Relationship rel = ped.GetRelationshipWithPed(this._ped);
            bool isTarget = rel == Relationship.Hate;

            //if (ClientInformation.IsDeveloper())
            //{
            //    if (ped.Handle == Game.PlayerPed.Handle)
            //    {
            //        rel = ped.GetRelationshipWithPed(this._ped);
            //        Screen.ShowSubtitle($"isT: {isTarget}, r: {rel}");
            //    }
            //    else
            //    {
            //        Screen.ShowSubtitle($"isT: {isTarget}");
            //    }

                
            //}

            return isTarget;
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
            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }

            Client.GetInstance().DeregisterTickHandler(OnDevUI);
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
