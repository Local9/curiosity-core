using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.ClientEvents;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Classes
{
    [Serializable]
    public class Ped : Entity, IEquatable<Ped>
    {
        public CitizenFX.Core.Ped Fx { get; set; }
        internal PluginManager Instance => PluginManager.Instance;
        internal EventSystem EventSystem => EventSystem.GetModule();
        public Vector3 Position => Fx.Position;
        public Tasks Task => Fx.Task;
        public bool IsDead => Fx.IsDead;
        internal string Name => Fx.Model.ToString();
        public bool IsBeingStunned => Fx.IsBeingStunned;
        public bool IsInVehicle { get; set; }
        private bool _DEBUG_ENABLED { get; set; } = false;

        public bool IsFleeing
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_FLEE);
            }
            set
            {
                this.IsArrestable = true;
                Decorators.Set(Fx.Handle, Decorators.PED_FLEE, value);
            }
        }

        public bool IsSuspect
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_SUSPECT);
            }
            set
            {
                this.IsArrestable = true;
                Decorators.Set(Fx.Handle, Decorators.PED_SUSPECT, value);
            }
        }

        public bool IsArrestable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_ARRESTABLE);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.PED_ARRESTABLE, value);
            }
        }

        public bool IsMission
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_MISSION);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.PED_MISSION, value);
            }
        }

        public bool HasDialogue
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_DIALOGUE);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.PED_DIALOGUE, value);
            }
        }

        public bool IsImportant
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_IMPORTANT);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.PED_IMPORTANT, value);
            }
        }

        public bool IsHostage
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_HOSTAGE);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.PED_HOSTAGE, value);
            }
        }

        public bool IsFriendly
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_FRIENDLY);
            }
            set
            {
                if (!value)
                {
                    Fx.RelationshipGroup = (uint)Collections.RelationshipHash.HatesPlayer;
                }
                else
                {
                    Fx.RelationshipGroup = (uint)Collections.RelationshipHash.NoRelationship;
                }

                Decorators.Set(Fx.Handle, Decorators.PED_FRIENDLY, !value);
            }
        }

        public bool IsReleased
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_RELEASED);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.PED_RELEASED, value);
            }
        }

        public bool IsHandcuffed
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_HANDCUFFED);
            }
            set
            {
                if (value)
                {
                    RunSequence(Sequence.HANDCUFF_APPLY);
                }
                else
                {
                    RunSequence(Sequence.HANDCUFF_REMOVE);
                }

                Decorators.Set(Fx.Handle, Decorators.PED_HANDCUFFED, value);
            }
        }

        public bool IsKneeling { get; set; }
        public bool IsGrabbed { get; set; }
        public DateTime LastUpdate { get; private set; }

        private EntityEventWrapper _eventWrapper;
        private long TimeOfDeath = 0;

        internal Ped(CitizenFX.Core.Ped fx, bool updateData = true) : base(fx.Handle)
        {
            Fx = fx;

            API.NetworkRegisterEntityAsNetworked(fx.Handle);

            this._eventWrapper = new EntityEventWrapper(this.Fx);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            Fx.AlwaysKeepTask = true;
            Fx.DropsWeaponsOnDeath = false;
            Fx.IsPersistent = true;
            Fx.Health = 200;

            API.SetPedFleeAttributes(Fx.Handle, 0, false);
            API.SetBlockingOfNonTemporaryEvents(Fx.Handle, true);
            API.TaskSetBlockingOfNonTemporaryEvents(Fx.Handle, true);
            API.SetPedDiesInWater(Fx.Handle, false);
            API.SetPedDiesWhenInjured(Fx.Handle, false);
            API.SetPedCombatAttributes(Fx.Handle, 17, false); // Flee if faced with weapon
            API.SetPedCombatAttributes(Fx.Handle, 46, false); // BF_AlwaysFight 
            API.SetPedCombatAttributes(Fx.Handle, 5, false); // BF_CanFightArmedPedsWhenNotArmed 
            Fx.SetConfigFlag(281, true); // No more rolling about

            API.NetworkFadeInEntity(fx.Handle, false);

            if (updateData)
                Instance.AttachTickHandler(OnUpdatePedTick);
        }

        private async Task OnUpdatePedTick()
        {
            if (DateTime.Now.Subtract(LastUpdate).TotalSeconds < 5)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (!Fx.Exists())
            {
                Instance.DetachTickHandler(OnUpdatePedTick);
                return;
            }

            LastUpdate = DateTime.Now;

            bool attachBlip = false;

            if (Fx.AttachedBlip != null)
                attachBlip = true;

            EventSystem.Request<bool>("mission:add:ped", Fx.NetworkId, IsSuspect, IsHandcuffed, attachBlip);
        }

        internal void PrisonTransport()
        {
            // PrisonerTransportManager.Collect(this);
        }

        internal async void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            bool flag;

            // if the ped is marked as a mission related ped, do not allow them to be deleted unless they match the other criteria
            // does require manual clean up also

            if (this.Position.VDist(Game.PlayerPed.Position) <= 120f || IsImportant)
            {
                flag = false;
            }
            else
            {
                flag = (!base.IsOnScreen ? true : base.IsDead && !IsImportant);
            }
            if (flag)
            {
                base.Delete();
            }

            if (Fx.IsBeingStunned && Fx.IsAlive && IsArrestable)
            {
                base.MaxHealth = 200;
                base.Health = 200;

                await BaseScript.Delay(10);

                if (Utility.RANDOM.Bool(0.75f) && !IsKneeling)
                {
                    RunSequence(Sequence.KNEEL);
                }
            }

            if (TimeOfDeath > 0) // Remove the ped from the world
            {
                if ((API.GetGameTimer() - TimeOfDeath) > 5000)
                {
                    API.NetworkFadeOutEntity(base.Handle, false, false);
                    
                    while(API.NetworkIsEntityFading(base.Handle))
                    {
                        await BaseScript.Delay(10);
                    }

                    Dismiss();
                }
            }

            if (Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG) && !_DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                Instance.AttachTickHandler(OnDeveloperOverlay);
                _DEBUG_ENABLED = true;
            }
            else if (!Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG) && _DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                _DEBUG_ENABLED = false;
                Instance.DetachTickHandler(OnDeveloperOverlay);
            }
        }

        async Task OnDeveloperOverlay()
        {
            // DebuggingTools.DrawData(this);
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            Dismiss();
        }

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            if (TimeOfDeath == 0)
                TimeOfDeath = API.GetGameTimer();

            Blip b = Fx.AttachedBlip;

            if (b != null)
            {
                if (b.Exists())
                    b.Delete();
            }

            if (base.IsOccluded)
            {
                Dismiss();
            }
        }

        internal async void RunSequence(Sequence sequence)
        {
            switch (sequence)
            {
                case Sequence.KNEEL:
                    Fx.CanRagdoll = true;
                    Fx.Weapons.RemoveAll();

                    TaskSequence kneelTaskSequence = new TaskSequence();
                    kneelTaskSequence.AddTask.PlayAnimation("random@arrests", "idle_2_hands_up", 8.0f, -1, AnimationFlags.StayInEndFrame);
                    kneelTaskSequence.AddTask.PlayAnimation("random@arrests", "kneeling_arrest_idle", 8.0f, -1, AnimationFlags.StayInEndFrame);
                    kneelTaskSequence.AddTask.PlayAnimation("random@arrests@busted", "enter", 8.0f, -1, AnimationFlags.StayInEndFrame);
                    kneelTaskSequence.AddTask.PlayAnimation("random@arrests@busted", "idle_a", 8.0f, -1, (AnimationFlags)9);
                    Fx.Task.PerformSequence(kneelTaskSequence);
                    kneelTaskSequence.Close();

                    IsKneeling = true;

                    break;
                case Sequence.UNKNEEL_AND_FLEE:
                    Fx.Task.ClearAllImmediately();
                    TaskSequence realeaseAndFleeTaskSequence = new TaskSequence();
                    realeaseAndFleeTaskSequence.AddTask.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8.0f, -1, AnimationFlags.CancelableWithMovement);
                    realeaseAndFleeTaskSequence.AddTask.ReactAndFlee(Game.PlayerPed);
                    Fx.Task.PerformSequence(realeaseAndFleeTaskSequence);
                    realeaseAndFleeTaskSequence.Close();
                    Fx.Task.ClearSecondary();
                    IsKneeling = false;

                    RunSequence(Sequence.REMOVE_FROM_WORLD);
                    break;
                case Sequence.REMOVE_FROM_WORLD:
                    while (Fx.Position.Distance(Game.PlayerPed.Position) < 25f)
                    {
                        await BaseScript.Delay(100);
                    }

                    int handle = Fx.Handle;
                    API.RemovePedElegantly(ref handle);

                    API.NetworkFadeOutEntity(base.Handle, false, false);
                    await BaseScript.Delay(2000);
                    Dismiss();

                    break;
                case Sequence.HANDCUFF_APPLY:
                    Game.PlayerPed.IsPositionFrozen = true;

                    API.SetPedFleeAttributes(Fx.Handle, 0, false);
                    API.SetBlockingOfNonTemporaryEvents(Fx.Handle, true);
                    API.TaskSetBlockingOfNonTemporaryEvents(Fx.Handle, true);

                    float y = IsKneeling ? 0.3f : 0.65f;
                    Vector3 pos = Vector3.Zero;
                    pos.Y = y;

                    Fx.AttachTo(Game.PlayerPed.Bones[11816], pos);

                    Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 4f, -1, (AnimationFlags)49);
                    Fx.Task.PlayAnimation("mp_arresting", "arrested_spin_l_0", 4f, -1, AnimationFlags.None);
                    await BaseScript.Delay(3000);
                    Fx.Task.PlayAnimation("mp_arresting", "idle", 8f, -1, (AnimationFlags)49);
                    await BaseScript.Delay(1000);

                    if (IsKneeling)
                    {
                        Fx.Task.PlayAnimation("random@arrests@busted", "exit", 8f, -1, AnimationFlags.StayInEndFrame);
                        await BaseScript.Delay(1000);
                        Fx.Task.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8f, -1, AnimationFlags.CancelableWithMovement);
                        this.IsKneeling = false;
                    }

                    await BaseScript.Delay(1000);

                    Game.PlayerPed.Task.ClearAll();

                    Fx.Detach();

                    API.SetEnableHandcuffs(Fx.Handle, true);

                    Game.PlayerPed.IsPositionFrozen = false;

                    Decorators.Set(Fx.Handle, Decorators.PED_ARRESTED, true);

                    break;
                case Sequence.HANDCUFF_REMOVE:

                    Game.PlayerPed.IsPositionFrozen = true;

                    IsKneeling = Fx.IsPlayingAnim("random@arrests@busted", "idle_a") || Fx.IsPlayingAnim("random@arrests@busted", "exit");

                    float kneelingY = IsKneeling ? 0.3f : 0.65f;
                    Vector3 kneelingPos = Vector3.Zero;
                    kneelingPos.Y = kneelingY;

                    Fx.AttachTo(Game.PlayerPed.Bones[11816], kneelingPos);

                    Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8f, -1, (AnimationFlags)49);

                    await BaseScript.Delay(1000);

                    Game.PlayerPed.Task.ClearAll();

                    API.SetEnableHandcuffs(Fx.Handle, false);
                    Fx.Detach();

                    Game.PlayerPed.IsPositionFrozen = false;

                    Decorators.Set(Fx.Handle, Decorators.PED_ARRESTED, false);

                    break;
                case Sequence.DETAIN_IN_CURRENT_VEHICLE:

                    if (PlayerManager.PersonalVehicle == null)
                    {
                        Screen.ShowNotification("~r~Vehicle not found.");
                        return;
                    }

                    if (PlayerManager.PersonalVehicle.IsSeatFree(VehicleSeat.LeftRear))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.PersonalVehicle, VehicleSeat.LeftRear);
                    }
                    else if (PlayerManager.PersonalVehicle.IsSeatFree(VehicleSeat.RightRear))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.PersonalVehicle, VehicleSeat.RightRear);
                    }
                    else if (PlayerManager.PersonalVehicle.IsSeatFree(VehicleSeat.Passenger))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.PersonalVehicle, VehicleSeat.Passenger);
                    }
                    else
                    {
                        Screen.ShowNotification("~r~Unable to find a free seat.");
                        return;
                    }

                    while (!Fx.IsInVehicle())
                    {
                        await BaseScript.Delay(100);
                    }

                    Fx.SetConfigFlag(292, true);

                    break;
                case Sequence.LEAVE_VEHICLE:
                    if (Fx.IsInVehicle())
                    {
                        Fx.SetConfigFlag(292, false);
                        Fx.Task.LeaveVehicle();
                    }
                    break;
                case Sequence.ARRESTED:
                    Ped myPed = Mission.RegisteredPeds.Select(x => x).Where(x => x.Handle == this.Handle).FirstOrDefault();
                    if (myPed != null)
                    {
                        // Mission.NumberArrested++;
                    }
                    break;
                case Sequence.GRAB_HOLD:
                    Vector3 attachedPos = new Vector3(-0.3f, 0.4f, 0.0f);
                    Fx.AttachTo(Game.PlayerPed, attachedPos);
                    API.SetBlockingOfNonTemporaryEvents(Fx.Handle, true);
                    IsGrabbed = true;
                    Fx.IsInvincible = true;
                    break;
                case Sequence.GRAB_RELEASE:
                    Fx.Detach();
                    IsGrabbed = false;
                    Fx.IsInvincible = false;
                    break;
            }
        }

        public static async Task<Ped> SpawnRandom(Vector3 position, float heading = 0f, bool sidewalk = true, PedType pedType = PedType.PED_TYPE_CIVMALE, bool isNetworked = true, bool isMission = true)
        {

            Vector3 spawnPosition = position;

            if (sidewalk)
            {
                spawnPosition = position.Sidewalk();
            }
            else
            {
                float groundZ = position.Z;
                Vector3 normal = Vector3.Zero;

                if (API.GetGroundZAndNormalFor_3dCoord(position.X, position.Y, position.Z, ref groundZ, ref normal))
                {
                    spawnPosition.Z = groundZ;
                }
            }

            Model model = Collections.Peds.ALL.Random();

            await model.Request(10000);

            while (!model.IsLoaded)
            {
                await BaseScript.Delay(100);
            }

            int pedId = API.CreatePed((int)pedType, (uint)model.Hash, spawnPosition.X, spawnPosition.Y, spawnPosition.Z, heading, isNetworked, isMission);

            CitizenFX.Core.Ped fxPed = new CitizenFX.Core.Ped(pedId);

            fxPed.FadeIn();

            Logger.Debug(fxPed.ToString());
            var ped = new Ped(fxPed);
            return ped;
        }

        public static async Task<Ped> Spawn(Model model, Vector3 position, float heading = 0f, bool sidewalk = true, PedType pedType = PedType.PED_TYPE_CIVMALE, bool isNetworked = true, bool isMission = true)
        {
            Vector3 spawnPosition = position;

            if (sidewalk)
            {
                spawnPosition = position.Sidewalk();
            }
            else
            {
                float groundZ = position.Z;
                Vector3 normal = Vector3.Zero;

                if (API.GetGroundZAndNormalFor_3dCoord(position.X, position.Y, position.Z, ref groundZ, ref normal))
                {
                    spawnPosition.Z = groundZ;
                }
            }

            await model.Request(10000);

            while(!model.IsLoaded)
            {
                await BaseScript.Delay(100);
            }

            int pedId = API.CreatePed((int)pedType, (uint)model.Hash, spawnPosition.X, spawnPosition.Y, spawnPosition.Z, heading, isNetworked, isMission);

            CitizenFX.Core.Ped fxPed = new CitizenFX.Core.Ped(pedId);

            fxPed.FadeIn();

            Logger.Debug(fxPed.ToString());
            var ped = new Ped(fxPed);
            return ped;
        }

        public void PutInVehicle(Vehicle vehicle, VehicleSeat seat = VehicleSeat.Driver) =>
            Fx.SetIntoVehicle(vehicle.Fx, seat);

        public Blip AttachBlip(BlipColor color, bool showRoute = false)
        {
            Blip blip = Fx.AttachBlip();
            blip.Color = color;
            blip.ShowRoute = showRoute;

            return blip;
        }

        internal async void Dismiss()
        {
            if (Fx == null) return;
            if (!base.Exists()) return;

            int handle = Fx.Handle;
            API.RemovePedElegantly(ref handle);

            await Fx.FadeOut();

            EventSystem.Request<bool>("mission:remove:ped", Fx.NetworkId);

            Instance.DetachTickHandler(OnUpdatePedTick);

            Blip singleBlip = Fx.AttachedBlip;
            if (singleBlip != null)
            {
                if (singleBlip.Exists())
                    singleBlip.Delete();
            }

            Fx.IsPersistent = false;
            Fx.MarkAsNoLongerNeeded();

            base.Delete();
        }

        protected bool Equals(Ped other)
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
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((Ped)obj) : true;
            }
            return flag;
        }

        public void ParticleEffect(string dict, string fx, Vector3 offset, float scale)
        {
            EntityHandler.ParticleEffect(NetworkId, dict, fx, offset, scale);
        }

        bool IEquatable<Ped>.Equals(Ped other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Fx, other.Fx));
        }

        internal enum Sequence
        {
            KNEEL,
            UNKNEEL,
            UNKNEEL_AND_FLEE,
            REMOVE_FROM_WORLD,
            HANDCUFF_APPLY,
            HANDCUFF_REMOVE,
            DETAIN_IN_CURRENT_VEHICLE,
            LEAVE_VEHICLE,
            ARRESTED,
            FOLLOW,
            GRAB_HOLD,
            GRAB_RELEASE
        }
    }
}
