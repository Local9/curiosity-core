using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Managers;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Utils;
using Newtonsoft.Json.Bson;
using System;
using System.Text.RegularExpressions;
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
        }

        internal async void Update(EntityEventWrapper entityEventWrapper, Entity entity)
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

            if (Fx.IsBeingStunned && Fx.IsAlive && IsArrestable)
            {
                base.MaxHealth = 200;
                base.Health = 200;

                if (Utility.RANDOM.Bool(0.9f) && !IsKneeling)
                {
                    RunSequence(Sequence.KNEEL);
                }
            }

            if (TimeOfDeath > 0) // Remove the ped from the world
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

            if (IsHostage)
                PluginIntance.RegisterTickHandler(OnPedInteractionCheck);

            if (Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG))
            {
                PluginIntance.RegisterTickHandler(OnDeveloperOverlay);
            }
            else
            {
                PluginIntance.DeregisterTickHandler(OnDeveloperOverlay);
            }
        }

        async Task OnDeveloperOverlay()
        {
            DebuggingTools.DrawData(this);
        }

        async Task OnPedInteractionCheck()
        {
            if (IsHostage) return;

            float distanceCheck = Fx.IsInVehicle() ? 3f : 1.5f;

            string message = $"Press ~INPUT_CONTEXT~ to interact";

            if (Game.PlayerPed.Position.Distance(Fx.Position) < distanceCheck)
            {
                Screen.DisplayHelpTextThisFrame($"{message}");

                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    // what do I want to do with you
                }
            }
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

                    PluginIntance.RegisterTickHandler(OnPedInteractionCheck);

                    break;
                case Sequence.UNKNEEL_AND_FLEE:
                    Fx.Task.ClearAllImmediately();
                    TaskSequence realeaseAndFleeTaskSequence = new TaskSequence();
                    // realeaseAndFleeTaskSequence.AddTask.PlayAnimation("random@arrests@busted", "exit", 8.0f, -1, AnimationFlags.StayInEndFrame);
                    realeaseAndFleeTaskSequence.AddTask.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8.0f, -1, AnimationFlags.CancelableWithMovement);
                    realeaseAndFleeTaskSequence.AddTask.ReactAndFlee(Game.PlayerPed);
                    Fx.Task.PerformSequence(realeaseAndFleeTaskSequence);
                    realeaseAndFleeTaskSequence.Close();
                    Fx.Task.ClearSecondary();
                    IsKneeling = false;
                    PluginIntance.DeregisterTickHandler(OnPedInteractionCheck);
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
            }
        }

        private async void ApplyHandcuffs()
        {
            if (Decorators.GetBoolean(Fx.Handle, Decorators.PED_ARREST))
            {
                if (Fx.IsCuffed)
                {
                    API.SetPedFleeAttributes(Fx.Handle, 0, false);

                    Game.PlayerPed.Task.TurnTo(Fx);
                    Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                    Fx.Task.PlayAnimation("mp_arresting", "idle", 8.0f, -1, (AnimationFlags)49);

                    float position = Fx.IsPlayingAnim("random@arrests@busted", "idle_a") ? 0.3f : 0.65f;
                    API.AttachEntityToEntity(Fx.Handle, Game.PlayerPed.Handle, 11816, 0.0f, position, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                    await BaseScript.Delay(2000);
                    Fx.Detach();
                    Fx.Task.ClearSecondary();
                    Game.PlayerPed.Task.ClearSecondary();

                    API.SetEnableHandcuffs(Fx.Handle, true);
                }
                else
                {
                    Game.PlayerPed.Task.TurnTo(Fx);
                    Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                    API.AttachEntityToEntity(Fx.Handle, Game.PlayerPed.Handle, 11816, 0.0f, 0.65f, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                    await BaseScript.Delay(2000);
                    Fx.Detach();
                    Fx.Task.ClearSecondary();
                    Game.PlayerPed.Task.ClearSecondary();
                }
            }
        }

        internal static async Task<Ped> Spawn(Model model, Vector3 position, bool sidewalk = true)
        {
            float groundZ = position.Z;

            Vector3 spawnPosition = position;

            if (sidewalk)
            {
                spawnPosition = position.Sidewalk();
            }

            Vector3 normal = Vector3.Zero;

            if (API.GetGroundZAndNormalFor_3dCoord(position.X, position.Y, position.Z, ref groundZ, ref normal))
            {
                spawnPosition.Z = groundZ;
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

            PluginIntance.DeregisterTickHandler(OnPedInteractionCheck);

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
            REMOVE_FROM_WORLD
        }
    }
}
