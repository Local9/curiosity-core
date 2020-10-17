using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Entity;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class NormalPed : Entity, IEquatable<Ped>
    {
        private static Client client = Client.GetInstance();

        public const int MovementUpdateInterval = 5;

        public static int Damage;

        public static float SensingRange;
        public static float SilencerEffectiveRange;
        public static float BehindNoticeDistance;
        public static float RunningNoticeDistance;
        public static float AttackRange;
        public static float VisionDistance;
        public static float WanderRadius;

        public static bool CanBeArrested = false;
        public static bool IsHostage = false;

        private Ped _target;

        private readonly Ped _ped;
        private EntityEventWrapper _eventWrapper;
        private bool _goingToTarget;
        private bool _attackingTarget;
        public List<Vector3> Waypoints;
        private int NextWaypoint = 0;
        public float Experience;

        private static string helpText = string.Empty;

        public bool IsInVehicle
        {
            get
            {
                return this._ped.IsInVehicle();
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
                if ((!value || this._ped.IsRagdoll || base.IsDead || this._ped.IsClimbing || this._ped.IsFalling || this._ped.IsBeingStunned ? false : !this._ped.IsGettingUp))
                {
                    NormalPed.OnAttackingTargetEvent onAttackingTargetEvent = this.AttackTarget;
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
                    NormalPed.OnGoingToTargetEvent onGoingToTargetEvent = this.GoToTarget;
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
                    this._ped.Task.WanderAround(this.Position, NormalPed.WanderRadius);
                    int num = 0;
                    bool flag = num == 1;
                    this.AttackingTarget = num == 1;
                    this.GoingToTarget = flag;
                }
                this._target = value;
            }
        }

        static NormalPed()
        {
            NormalPed.Damage = 15;
            NormalPed.SensingRange = 120f;
            NormalPed.SilencerEffectiveRange = 15f;
            NormalPed.BehindNoticeDistance = 5f;
            NormalPed.RunningNoticeDistance = 25f;
            NormalPed.WanderRadius = 100f;

            client.RegisterTickHandler(ShowHelpText);
        }

        protected NormalPed(int handle, float visionDistance = 35f, float experience = 10f) : base(handle)
        {
            this._ped = new Ped(handle);
            Create(handle, false, visionDistance, experience);
        }

        protected NormalPed(int handle, bool canArrest = false, float visionDistance = 35f, float experience = 10f) : base(handle)
        {
            this._ped = new Ped(handle);
            Create(handle, canArrest, visionDistance, experience);
        }

        private void Create(int handle, bool canArrest = false, float visionDistance = 35f, float experience = 10f)
        {
            this._eventWrapper = new EntityEventWrapper(this._ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            Handle = handle;
            VisionDistance = visionDistance;
            AttackRange = visionDistance;
            Experience = experience;
            CanBeArrested = canArrest;

            NormalPed NormalPed = this;
            this.GoToTarget += new NormalPed.OnGoingToTargetEvent(NormalPed.OnGoToTarget);
            NormalPed MissionPed1 = this;
            this.AttackTarget += new NormalPed.OnAttackingTargetEvent(MissionPed1.OnAttackTarget);
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();
        }

        private bool CanHearPed(Ped ped)
        {
            float single = ped.Position.VDist(this.Position);
            return (!NormalPed.IsWeaponWellSilenced(ped, single) || NormalPed.IsBehind(single) ? true : NormalPed.IsRunningNoticed(ped, single));
        }

        protected bool Equals(NormalPed other)
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
                flag = (obj.GetType() != base.GetType() ? false : this.Equals((NormalPed)obj));
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
                flag = (this._ped.HasClearLineOfSight(closest, NormalPed.VisionDistance) ? true : this.CanHearPed(closest));
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
            return distance < NormalPed.BehindNoticeDistance;
        }

        private bool IsGoodTarget(Ped ped)
        {
            return ped.GetRelationshipWithPed(this._ped) == Relationship.Hate;
        }

        private static bool IsRunningNoticed(Ped ped, float distance)
        {
            return (!ped.IsSprinting ? false : distance < NormalPed.RunningNoticeDistance);
        }

        private static bool IsWeaponWellSilenced(Ped ped, float distance)
        {
            bool flag;
            if (ped.IsShooting)
            {
                flag = (!ped.IsCurrentWeaponSileced() ? false : distance > NormalPed.SilencerEffectiveRange);
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

            Entity killerEnt = new Ped(entity.Handle).GetKiller();

            if (killerEnt != null)
            {
                Ped killerPed = new Ped(killerEnt.Handle);

                if (killerPed != null)
                {
                    if (killerPed.IsPlayer)
                    {
                        Player p = new Player(CitizenFX.Core.Native.API.NetworkGetPlayerIndexFromPed(killerPed.Handle));

                        SkillMessage skillMessage = new SkillMessage();
                        skillMessage.PlayerHandle = $"{p.ServerId}";
                        skillMessage.MissionPed = false;
                        skillMessage.Increase = false;

                        string json = JsonConvert.SerializeObject(skillMessage);

                        BaseScript.TriggerServerEvent("curiosity:Server:Missions:KilledPed", Encode.StringToBase64(json));
                    }
                }
            }

            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }

            await Client.Delay(3000);
            API.NetworkFadeOutEntity(this.Handle, false, false);
            await Client.Delay(1000);

            sender.Dispose();
        }

        public abstract void OnGoToTarget(Ped target);

        public static implicit operator Ped(NormalPed v)
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

            if (CanBeArrested)
                this.CheckIfArrested();

            if (Waypoints != null)
                this.GotoWaypoint();

            if (this.Target != null)
            {
                if (this.Position.VDist(this.Target.Position) <= NormalPed.AttackRange)
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

        private async void CheckIfArrested()
        {

        }

        private static async Task ShowHelpText()
        {
            if (!string.IsNullOrEmpty(helpText))
                CitizenFX.Core.UI.Screen.DisplayHelpTextThisFrame(helpText);
        }

        public event NormalPed.OnAttackingTargetEvent AttackTarget;

        public event NormalPed.OnGoingToTargetEvent GoToTarget;

        public delegate void OnAttackingTargetEvent(Ped target);

        public delegate void OnGoingToTargetEvent(Ped target);
    }
}
