using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Managers;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Classes
{
    [Serializable]
    internal class Ped : Entity, IEquatable<Ped>
    {
        private DebuggingTools DebuggingTools = new DebuggingTools();
        internal CitizenFX.Core.Ped Fx { get; set; }
        internal PluginManager PluginIntance => PluginManager.Instance;
        public Vector3 Position => Fx.Position;
        internal Tasks Task => Fx.Task;
        internal bool IsDead => Fx.IsDead;
        internal string Name => Fx.Model.ToString();
        internal bool IsBeingStunned => Fx.IsBeingStunned;
        internal bool IsInVehicle { get; set; }
        private bool _DEBUG_ENABLED { get; set; } = false;

        internal bool IsSuspect
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

        internal bool IsArrestable
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

        internal bool IsMission
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

        internal bool IsImportant
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

        internal bool IsHostage
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

        internal bool IsReleased
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

        internal bool IsHandcuffed
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.PED_HANDCUFFED);
            }
            set
            {
                if (value)
                {
                    RunSequence(Sequence.HANDCUFFED);
                }
                else
                {
                    RunSequence(Sequence.UNHANDCUFFED);
                }

                Decorators.Set(Fx.Handle, Decorators.PED_HANDCUFFED, value);
            }
        }

        internal bool IsKneeling { get; set; }

        private EntityEventWrapper _eventWrapper;
        private long TimeOfDeath = 0;

        internal Ped(CitizenFX.Core.Ped fx) : base(fx.Handle)
        {
            Fx = fx;

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
            API.SetPedCombatAttributes(Fx.Handle, 17, false);
            API.SetPedCombatAttributes(Fx.Handle, 46, false);
            API.SetPedCombatAttributes(Fx.Handle, 5, false);
            Fx.SetConfigFlag(281, true); // No more rolling about

            API.RequestAnimDict("mp_arresting");
            API.RequestAnimDict("random@arrests@busted");
            API.RequestAnimDict("random@arrests");
        }

        internal void PrisonTransport()
        {
            PrisonerTransportManager.Collect(this);
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

                if (Utility.RANDOM.Bool(0.8f) && !IsKneeling)
                {
                    RunSequence(Sequence.KNEEL);
                }
            }

            if (TimeOfDeath > 0 && !IsImportant) // Remove the ped from the world
            {
                if ((API.GetGameTimer() - TimeOfDeath) > 5000)
                {
                    int handle = Fx.Handle;
                    API.RemovePedElegantly(ref handle);

                    API.NetworkFadeOutEntity(base.Handle, false, false);
                    await BaseScript.Delay(2000);
                    Dismiss();
                }
            }

            if (Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG) && !_DEBUG_ENABLED)
            {
                PluginIntance.RegisterTickHandler(OnDeveloperOverlay);
                _DEBUG_ENABLED = true;
            }
            else if (!Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG) && _DEBUG_ENABLED)
            {
                _DEBUG_ENABLED = false;
                PluginIntance.DeregisterTickHandler(OnDeveloperOverlay);
            }
        }

        async Task OnDeveloperOverlay()
        {
            DebuggingTools.DrawData(this);
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            Dismiss();
        }

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            if (TimeOfDeath == 0)
                TimeOfDeath = API.GetGameTimer();

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
                case Sequence.HANDCUFFED:
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

                    break;
                case Sequence.UNHANDCUFFED:

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

                    break;
                case Sequence.DETAIN_IN_CURRENT_VEHICLE:

                    if (PlayerManager.Vehicle == null)
                    {
                        Screen.ShowNotification("~r~Vehicle not found.");
                        return;
                    }

                    if (PlayerManager.Vehicle.IsSeatFree(VehicleSeat.LeftRear))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.Vehicle, VehicleSeat.LeftRear);
                    }
                    else if (PlayerManager.Vehicle.IsSeatFree(VehicleSeat.RightRear))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.Vehicle, VehicleSeat.RightRear);
                    }
                    else if (PlayerManager.Vehicle.IsSeatFree(VehicleSeat.Passenger))
                    {
                        Fx.Task.EnterVehicle(PlayerManager.Vehicle, VehicleSeat.Passenger);
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
                        Fx.SetConfigFlag(293, false);
                        Fx.Task.LeaveVehicle();
                    }
                    break;
                case Sequence.ARRESTED:
                    Callout callout = CalloutManager.ActiveCallout;
                    Ped myPed = callout.RegisteredPeds.Select(x => x).Where(x => x.Handle == this.Handle).FirstOrDefault();
                    if (myPed != null)
                    {
                        callout.NumberArrested++;
                    }
                    break;
            }
        }
        internal static async Task<Ped> SpawnRandom(Vector3 position, bool sidewalk = true)
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

            CitizenFX.Core.Ped fxPed = await World.CreatePed(Collections.Peds.ALL.Random(), spawnPosition);

            API.NetworkFadeInEntity(fxPed.Handle, false);

            Logger.Log(fxPed.ToString());
            var ped = new Ped(fxPed);
            return ped;
        }

        internal static async Task<Ped> Spawn(Model model, Vector3 position, bool sidewalk = true)
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

            CitizenFX.Core.Ped fxPed = await World.CreatePed(model, spawnPosition);

            API.NetworkFadeInEntity(fxPed.Handle, false);

            Logger.Log(fxPed.ToString());
            var ped = new Ped(fxPed);
            return ped;
        }

        internal void PutInVehicle(Vehicle vehicle, VehicleSeat seat = VehicleSeat.Driver) =>
            Fx.SetIntoVehicle(vehicle.Fx, seat);

        internal Blip AttachBlip(BlipColor color, bool showRoute = false)
        {
            Blip blip = Fx.AttachBlip();
            blip.Color = color;
            blip.ShowRoute = showRoute;

            return blip;
        }

        internal void Dismiss()
        {
            //if (Fx.AttachedBlips.Length > 0)
            //    foreach (Blip blip in Fx.AttachedBlips) blip.Delete();
            API.RemoveAnimDict("mp_arresting");
            API.RemoveAnimDict("random@arrests@busted");
            API.RemoveAnimDict("random@arrests");

            if (Fx == null) return;
            if (!base.Exists()) return;

            Blip singleBlip = Fx.AttachedBlip;
            if (singleBlip != null)
            {
                if (singleBlip.Exists())
                    singleBlip.Delete();
            }

            Fx.IsPersistent = false;

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
            HANDCUFFED,
            UNHANDCUFFED,
            DETAIN_IN_CURRENT_VEHICLE,
            LEAVE_VEHICLE,
            ARRESTED
        }
    }
}
