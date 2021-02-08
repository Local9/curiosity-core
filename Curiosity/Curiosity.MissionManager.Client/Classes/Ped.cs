using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.ClientEvents;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
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

        internal DateTime DateCreated { get; set; }

        public void AddToMission()
        {
            EventSystem.Send("mission:add:ped", Fx.NetworkId, (int)Fx.Gender, IsDriver);
        }

        public bool IsInVehicle => Fx.IsInVehicle();
        private bool _DEBUG_ENABLED { get; set; } = false;
        private bool isRandomPed { get; set; } = false;
        public bool IsDriver
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_IS_DRIVER);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.PED_IS_DRIVER, value);
                EventSystem.Send("mission:update:ped:driver", Fx.NetworkId, value);
            }
        }

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

                EventSystem.Send("mission:update:ped:suspect", Fx.NetworkId, value);
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
                Fx.IsPersistent = value;
                Decorators.Set(Fx.Handle, Decorators.PED_MISSION, value);

                EventSystem.Send("mission:update:ped:mission", Fx.NetworkId, value);
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

                EventSystem.Send("mission:update:ped:handcuffed", Fx.NetworkId, value);
            }
        }

        public bool IsKneeling { get; set; }
        public bool IsGrabbed { get; set; }
        public DateTime LastUpdate { get; private set; }
        public string Identity { get; internal set; }
        PedState pedState = PedState.NORMAL;

        private EntityEventWrapper _eventWrapper;
        private DateTime TimeOfDeath = new DateTime(1900, 1, 1);
        private bool isRemoving;

        public Ped(CitizenFX.Core.Ped fx, bool update = true, bool isRandomPed = false) : base(fx.Handle)
        {
            Fx = fx;

            API.NetworkRegisterEntityAsNetworked(fx.Handle);

            if (!isRandomPed)
                API.NetworkRequestControlOfEntity(fx.Handle);

            this._eventWrapper = new EntityEventWrapper(this.Fx);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            this.isRandomPed = isRandomPed;
            fx.Health = 200;

            fx.AlwaysDiesOnLowHealth = false;
            fx.DropsWeaponsOnDeath = false;

            DateCreated = DateTime.Now;

            if (!this.isRandomPed)
            {
                Fx.AlwaysKeepTask = true;
                Fx.IsPersistent = true;

                API.SetPedFleeAttributes(Fx.Handle, 0, false);
                API.SetBlockingOfNonTemporaryEvents(Fx.Handle, true);
                API.TaskSetBlockingOfNonTemporaryEvents(Fx.Handle, true);
                API.SetPedCombatAttributes(Fx.Handle, 17, false); // Flee if faced with weapon
                API.SetPedCombatAttributes(Fx.Handle, 46, false); // BF_AlwaysFight 
                API.SetPedCombatAttributes(Fx.Handle, 5, false); // BF_CanFightArmedPedsWhenNotArmed 
            }

            Fx.SetConfigFlag(281, true); // No more rolling about
            API.SetPedDiesInWater(Fx.Handle, false);
            API.SetPedDiesWhenInjured(Fx.Handle, false);

            IsDriver = fx.IsInVehicle() && fx.CurrentVehicle.Driver.Handle == fx.Handle;

            Decorators.Set(fx.Handle, Decorators.PED_SETUP, true);
            Decorators.Set(fx.Handle, Decorators.MENU_RANDOM_RESPONSE, Utility.RANDOM.Next(4));

            if (update)
                EventSystem.Send("mission:add:ped", fx.NetworkId, (int)fx.Gender, IsDriver);
        }

        public void AttachSuspectBlip()
        {
            if (Fx.AttachedBlip != null) return;

            API.SetPedAiBlip(Fx.Handle, true);
            API.IsAiBlipAlwaysShown(Fx.Handle, false);
            API.SetAiBlipMaxDistance(Fx.Handle, 50f);
            API.HideSpecialAbilityLockonOperation(Fx.Handle, false);
            API.SetAiBlipType(Fx.Handle, 0);

            EventSystem.Send("mission:update:ped:blip", Fx.NetworkId, true);
        }

        internal async void ArrestPed()
        {
            if (!IsHandcuffed)
            {
                Notify.Alert(CommonErrors.MustBeHandcuffed);
                return;
            }

            if (Fx.IsInVehicle())
            {
                Notify.Alert(CommonErrors.NpcOutsideVehicle);
                return;
            }

            // Dismiss after informing the server
            bool result = await EventSystem.GetModule().Request<bool>("mission:update:ped:arrest", Fx.NetworkId);

            if (result)
            {
                Dismiss();
            }
            else
            {
                Notify.Alert("Sorry, having issues logging this arrest.");
            }
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

            if (Fx.IsBeingStunned && Fx.IsAlive)
            {
                base.MaxHealth = 200;
                base.Health = 200;

                await BaseScript.Delay(10);

                if (Utility.RANDOM.Bool(0.75f) && !IsKneeling && IsArrestable)
                {
                    RunSequence(Sequence.KNEEL);
                }
            }

            if (TimeOfDeath.Year != 1900) // Remove the ped from the world
            {
                if (DateTime.Now.Subtract(TimeOfDeath).TotalSeconds > 5)
                {
                    Dismiss();
                    return;
                }
            }

            if (Fx.IsInVehicle() && pedState.Equals(PedState.NORMAL))
            {
                CitizenFX.Core.Vehicle vehicle = Fx.CurrentVehicle;

                float roll = API.GetEntityRoll(vehicle.Handle);
                if ((roll > 75.0f || roll < -75.0f) && vehicle.Speed < 4f)
                {
                    FleeVehicleAndFleeFromPlayer();
                }

                if (vehicle.Health < 200)
                {
                    FleeVehicleAndFleeFromPlayer();
                }
            }

            if (Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG_NPC) && !_DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                Instance.AttachTickHandler(OnDeveloperOverlay);
                _DEBUG_ENABLED = true;
            }
            else if (!Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG_NPC) && _DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                _DEBUG_ENABLED = false;
                Instance.DetachTickHandler(OnDeveloperOverlay);
            }
        }

        private void FleeVehicleAndFleeFromPlayer()
        {
            pedState = PedState.LEAVE_VEHICLE;

            TaskSequence taskSequence = new TaskSequence();

            if (Fx.IsInVehicle() && Fx.CurrentVehicle.Speed > 10f)
            {
                taskSequence.AddTask.LeaveVehicle(LeaveVehicleFlags.BailOut);
            }
            else
            {
                taskSequence.AddTask.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
            }

            taskSequence.AddTask.FleeFrom(Game.PlayerPed);
            Task.PerformSequence(taskSequence);
            taskSequence.Close();
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

        public async void RunSequence(Sequence sequence)
        {
            switch (sequence)
            {
                case Sequence.FLEE_IN_VEHICLE:
                    Fx.Task.ClearAll();
                    
                    Fx.Task.CruiseWithVehicle(Fx.CurrentVehicle, float.MaxValue,
                        (int)Collections.CombinedVehicleDrivingFlags.Fleeing);

                    await BaseScript.Delay(10000);

                    break;
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
                        Notify.Alert("~r~Vehicle not found, you must have a personal vehicle.");
                        return;
                    }

                    if (PlayerManager.PersonalVehicle.IsSeatFree(VehicleSeat.LeftRear))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.PersonalVehicle, VehicleSeat.LeftRear, 10000);
                    }
                    else if (PlayerManager.PersonalVehicle.IsSeatFree(VehicleSeat.RightRear))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.PersonalVehicle, VehicleSeat.RightRear, 10000);
                    }
                    else if (PlayerManager.PersonalVehicle.IsSeatFree(VehicleSeat.Passenger))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.PersonalVehicle, VehicleSeat.Passenger, 10000);
                    }
                    else
                    {
                        Notify.Alert("~r~Unable to find a free seat.");
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
                    Fx.IsCollisionEnabled = false;
                    break;
                case Sequence.GRAB_RELEASE:
                    Fx.Detach();
                    IsGrabbed = false;
                    Fx.IsInvincible = false;
                    Fx.IsCollisionEnabled = true;
                    break;
            }
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

            if (!isRandomPed)
                EventSystem.Request<bool>("mission:remove:ped", Fx.NetworkId);

            Blip singleBlip = Fx.AttachedBlip;
            if (singleBlip != null)
            {
                if (singleBlip.Exists())
                    singleBlip.Delete();
            }

            Fx.IsPersistent = false;
            Fx.MarkAsNoLongerNeeded();

            EventSystem.Send("entity:delete", Fx.NetworkId);

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

        public enum Sequence
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
            GRAB_RELEASE,
            FLEE_IN_VEHICLE
        }

        private enum PedState
        {
            NORMAL,
            LEAVE_VEHICLE,
            LEAVING_VEHICLE
        }
    }
}
