﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.ClientEvents;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Classes
{
    [Serializable]
    public class Vehicle : Entity, IEquatable<Vehicle>
    {
        private bool _canExecuteAnimation = true;

        public CitizenFX.Core.Vehicle Fx { get; private set; }
        public Vector3 Position => Fx.Position;
        public string Hash => Fx.Model.ToString();

        public void AddToMission()
        {
            EventSystem.Send("mission:add:vehicle", Fx.NetworkId);
        }

        private PluginManager Instance => PluginManager.Instance;
        internal EventSystem EventSystem => EventSystem.GetModule();

        private DateTime TimeOfDeath = new DateTime(1900, 1, 1);

        private EntityEventWrapper _eventWrapper;
        private bool _DEBUG_ENABLED;

        public string Name => API.GetLabelText(API.GetDisplayNameFromVehicleModel((uint)Fx.Model.Hash));

        public bool IsImportant
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_IMPORTANT);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_IMPORTANT, value);
            }
        }

        public bool IsTowable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_TOW);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_TOW, value);

                EventSystem.Send("mission:update:vehicle:towable", Fx.NetworkId, true);
            }
        }

        public bool IsSearchable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_SEARCH);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_SEARCH, value);
            }
        }

        public bool IsMission
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_MISSION);
            }
            set
            {
                Fx.IsPersistent = value;
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_MISSION, value);

                EventSystem.Send("mission:update:vehicle:mission", Fx.NetworkId, true);
            }
        }

        public bool IsSpikable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_SPIKE_ALLOWED);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_SPIKE_ALLOWED, value);
            }
        }

        public bool IsIgnored
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_TRAFFIC_STOP_IGNORED);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_TRAFFIC_STOP_IGNORED, value);
            }
        }

        public DateTime LastUpdate { get; private set; }

        public Vehicle(CitizenFX.Core.Vehicle fx, bool update = true) : base(fx.Handle)
        {
            Fx = fx;
            API.NetworkRegisterEntityAsNetworked(fx.Handle);

            this._eventWrapper = new EntityEventWrapper(this.Fx);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            Decorators.Set(fx.Handle, Decorators.VEHICLE_SETUP, true);

            if (update)
                EventSystem.Send("mission:add:vehicle", Fx.NetworkId);
        }

        public Blip AttachSuspectBlip()
        {
            if (Fx.AttachedBlip != null) return null;

            Blip blip = Fx.AttachBlip();
            blip.Color = BlipColor.Red;
            blip.Scale = .75f;

            EventSystem.Send("mission:update:vehicle:blip", Fx.NetworkId, true);

            return blip;
        }

        public static async Task<Vehicle> Spawn(Model model, Vector3 position, float heading = 0f, bool streetSpawn = true, bool isNetworked = true, bool isMission = true)
        {
            Vector3 spawnPosition = position;

            if (streetSpawn)
                spawnPosition = position.Street();

            await model.Request(10000);

            while (!model.IsLoaded) await BaseScript.Delay(100);

            int vehicleId = API.CreateVehicle((uint)model.Hash, spawnPosition.X, spawnPosition.Y, spawnPosition.Z, heading, isNetworked, isMission);

            CitizenFX.Core.Vehicle fxVehicle = new CitizenFX.Core.Vehicle(vehicleId);
            
            API.ClearAreaOfEverything(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, 5f, false, false, false, false);

            Logger.Debug(fxVehicle.ToString());

            fxVehicle.FadeIn();

            return new Vehicle(fxVehicle);
        }

        internal async void Dismiss()
        {
            if (Fx.AttachedBlip != null)
            {
                if (Fx.AttachedBlip.Exists())
                    Fx.AttachedBlip.Delete();
            }

            EventSystem.Request<bool>("mission:remove:vehicle", Fx.NetworkId);

            Fx.IsPersistent = false;
            Fx.MarkAsNoLongerNeeded();

            EventSystem.Send("entity:s:delete", Fx.NetworkId);

            await Fx.FadeOut();

            base.Delete();
        }

        protected bool Equals(Vehicle other)
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
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((Vehicle)obj) : true;
            }
            return flag;
        }

        bool IEquatable<Vehicle>.Equals(Vehicle other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Fx, other.Fx));
        }

        public void BurstWheel(Wheels wheel, bool onRim = false, float dmg = 1000f)
        {
            API.SetVehicleTyreBurst(Fx.Handle, (int)wheel, onRim, dmg);
        }

        public void DamageTop(float force = 1000f, float radius = 1000f, int numberOfHits = 1)
        {
            EntityHandler.TriggerDamageEvent(NetworkId, 0f, 1f, 1f, force, radius, true, numberOfHits);
        }

        public async void DamageFront(bool increaseDamage = false)
        {
            float force = 1600f;
            float radius = 1600f;

            EntityHandler.TriggerDamageEvent(NetworkId, 0f, 1.2f, 0f, force, radius, true, 1);
            await BaseScript.Delay(10);
            EntityHandler.TriggerDamageEvent(NetworkId, 0f, 0.75f, 0.05f, force, radius, true, 1);
            await BaseScript.Delay(10);
            EntityHandler.TriggerDamageEvent(NetworkId, -0.7f, 0f, 0f, force, radius, true, 1);
            await BaseScript.Delay(10);
            EntityHandler.TriggerDamageEvent(NetworkId, 0.7f, 0f, 0f, force, radius, true, 1);

            if (increaseDamage)
            {
                await BaseScript.Delay(10);
                EntityHandler.TriggerDamageEvent(NetworkId, 0f, 1.2f, 0f, force, radius, true, 1);
                await BaseScript.Delay(10);
                EntityHandler.TriggerDamageEvent(NetworkId, 0f, 0.75f, 0.05f, force, radius, true, 1);
                await BaseScript.Delay(10);
                EntityHandler.TriggerDamageEvent(NetworkId, -0.7f, 0f, 0f, force, radius, true, 1);
                await BaseScript.Delay(10);
                EntityHandler.TriggerDamageEvent(NetworkId, 0.7f, 0f, 0f, force, radius, true, 1);
            }
        }

        public void ParticleEffect(string dict, string fx, Vector3 offset, float scale)
        {
            EntityHandler.ParticleEffect(NetworkId, dict, fx, offset, scale);
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

            if (TimeOfDeath.Year != 1900)
            {
                if (DateTime.Now.Subtract(TimeOfDeath).TotalSeconds > 5)
                {
                    Dismiss();
                    return;
                }
            }

            if (Game.PlayerPed.CurrentVehicle == PlayerManager.PersonalVehicle && Game.PlayerPed.IsInVehicle())
            {
                if (Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Emergency
                && !Mission.isOnMission && !IsIgnored)
                {
                    bool isMarked = Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_TRAFFIC_STOP_MARKED);

                    if (Utility.RANDOM.Bool(0.1f) && !isMarked)
                    {
                        Decorators.Set(Fx.Handle, Decorators.VEHICLE_TRAFFIC_STOP_PULLOVER, true);
                    }

                    Decorators.Set(Fx.Handle, Decorators.VEHICLE_TRAFFIC_STOP_MARKED, true);

                    CitizenFX.Core.Vehicle playerVeh = PlayerManager.PersonalVehicle;

                    if (playerVeh.GetVehicleInFront(10f, 1f) == this.Fx && Fx.Driver != null && TrafficStopManager.Manager.tsVehicle == null)
                    {
                        HelpMessage.CustomLooped(HelpMessage.Label.TRAFFIC_STOP_INITIATE);

                        API.DisableControlAction(0, (int)Control.VehicleRadioWheel, true);
                        API.DisableControlAction(0, (int)Control.VehicleNextRadio, true);
                        API.DisableControlAction(0, (int)Control.VehicleNextRadioTrack, true);
                        API.DisableControlAction(0, (int)Control.VehiclePrevRadio, true);
                        API.DisableControlAction(0, (int)Control.VehiclePrevRadioTrack, true);

                        if (ControlHelper.IsControlJustPressed(Control.Context, false))
                        {
                            TrafficStopManager.Manager.SetVehicle(this);

                            for (int i = 0; i < 8; i++)
                            {
                                Game.PlayerPed.CurrentVehicle.IsSirenActive = i % 2 == 0;
                                await BaseScript.Delay(100);
                            }
                            Game.PlayerPed.CurrentVehicle.IsSirenActive = false;
                        }

                        if (ControlHelper.IsControlJustPressed(Control.Cover, false))
                            IsIgnored = true;
                    }
                }
            }

            if (Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG_VEH) && !_DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                Instance.AttachTickHandler(OnDeveloperOverlay);
                _DEBUG_ENABLED = true;
            }
            else if (!Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_DEBUG_VEH) && _DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                _DEBUG_ENABLED = false;
                Instance.DetachTickHandler(OnDeveloperOverlay);
            }
        }

        async Task OnDeveloperOverlay()
        {
            this.DrawData();
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            Dismiss();
        }

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            TimeOfDeath = DateTime.Now;

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

        public async void Sequence(VehicleSequence vehicleSequence)
        {
            switch (vehicleSequence)
            {
                case VehicleSequence.SEARCH:

                    if (!_canExecuteAnimation) return;

                    string[] vehicleDoorNames = new string[] { "handle_dside_f", "handle_dside_r", "handle_pside_f", "handle_pside_r", "bonnet", "boot" };
                    string closestDoor = "";
                    double num = 100;
                    string[] vehicleDoorNamesCopy = vehicleDoorNames;

                    for (int i = 0; i < (int)vehicleDoorNamesCopy.Length; i++)
                    {
                        string boneName = vehicleDoorNamesCopy[i];
                        int entityBoneIndex = API.GetEntityBoneIndexByName(Handle, boneName);
                        if (entityBoneIndex != -1)
                        {
                            Vector3 worldPositioinOfBone = API.GetWorldPositionOfEntityBone(Handle, entityBoneIndex);
                            float single = Vector3.Distance(worldPositioinOfBone, Game.PlayerPed.Position);
                            if ((double)single < num)
                            {
                                num = (double)single;
                                closestDoor = boneName;
                            }
                        }
                    }

                    if (num > 5)
                    {
                        Screen.ShowNotification("Sorry, unable to search this door");
                        return;
                    }

                    if (closestDoor == "handle_dside_f")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 0) < 0.25f)
                        {
                            Game.PlayerPed.Heading = Fx.Heading - 90f;
                            Game.PlayerPed.Task.PlayAnimation("veh@low@front_ds@enter_exit", "d_open_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.FrontLeftDoor].Open();
                            await BaseScript.Delay(500);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Game.PlayerPed.Heading = Fx.Heading;
                            Game.PlayerPed.Task.PlayAnimation("veh@low@front_ds@enter_exit", "d_close_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.FrontLeftDoor].Close();
                        }
                    }
                    else if (closestDoor == "handle_dside_r")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 2) < 0.25f)
                        {
                            Game.PlayerPed.Heading = Fx.Heading - 90f;
                            Game.PlayerPed.Task.PlayAnimation("veh@low@front_ds@enter_exit", "d_open_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.BackLeftDoor].Open();
                            await BaseScript.Delay(500);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Game.PlayerPed.Heading = Fx.Heading;
                            Game.PlayerPed.Task.PlayAnimation("veh@low@front_ds@enter_exit", "d_close_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.BackLeftDoor].Close();
                        }
                    }
                    else if (closestDoor == "handle_pside_f")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 1) < 0.25f)
                        {
                            Game.PlayerPed.Heading = Fx.Heading + 90f;
                            Game.PlayerPed.Task.PlayAnimation("veh@low@front_ps@enter_exit", "d_open_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.FrontRightDoor].Open();
                            await BaseScript.Delay(500);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Game.PlayerPed.Heading = Fx.Heading;
                            Game.PlayerPed.Task.PlayAnimation("veh@low@front_ps@enter_exit", "d_close_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.FrontRightDoor].Close();
                        }
                    }
                    else if (closestDoor == "handle_pside_r")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 3) < 0.25f)
                        {
                            Game.PlayerPed.Heading = Fx.Heading + 90f;
                            Game.PlayerPed.Task.PlayAnimation("veh@low@front_ps@enter_exit", "d_open_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.BackRightDoor].Open();
                            await BaseScript.Delay(500);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Game.PlayerPed.Heading = Fx.Heading;
                            Game.PlayerPed.Task.PlayAnimation("veh@low@front_ps@enter_exit", "d_close_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.BackRightDoor].Close();
                        }
                    }
                    else if (closestDoor == "bonnet")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 4) < 0.25f)
                        {
                            Game.PlayerPed.Heading = Fx.Heading - 180f;
                            Game.PlayerPed.Task.PlayAnimation("rcmnigel3_trunk", "out_trunk_trevor", 4f, 2500, AnimationFlags.None);
                            await BaseScript.Delay(750);
                            Fx.Doors[VehicleDoorIndex.Hood].Open();
                            await BaseScript.Delay(1000);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Game.PlayerPed.Heading = Fx.Heading - 180f;
                            Game.PlayerPed.Task.PlayAnimation("rcmepsilonism8", "bag_handler_close_trunk_walk_left", 4f, 2500, AnimationFlags.None);
                            await BaseScript.Delay(1250);
                            Fx.Doors[VehicleDoorIndex.Hood].Close();
                        }
                    }
                    else if (closestDoor == "boot")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 5) < 0.25f)
                        {
                            Game.PlayerPed.Heading = Fx.Heading;
                            Game.PlayerPed.Task.PlayAnimation("rcmnigel3_trunk", "out_trunk_trevor", 4f, 2500, AnimationFlags.None);
                            await BaseScript.Delay(750);
                            Fx.Doors[VehicleDoorIndex.Trunk].Open();
                            await BaseScript.Delay(1000);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Game.PlayerPed.Heading = Fx.Heading;
                            Game.PlayerPed.Task.PlayAnimation("rcmepsilonism8", "bag_handler_close_trunk_walk_left", 4f, 2500, AnimationFlags.None);
                            await BaseScript.Delay(1250);
                            Fx.Doors[VehicleDoorIndex.Trunk].Close();
                        }
                    }

                    break;
            }
        }

        public enum VehicleSequence
        {
            SEARCH
        }

        public enum Wheels
        {
            FRONT_LEFT = 0,
            FRONT_RIGHT = 1,
            MID_LEFT = 2,
            MID_RIGHT = 3,
            REAR_LEFT = 4,
            REAR_RIGHT = 5,
            TRAILER_MID_LEFT = 45,
            TRAILER_MID_RIGHT = 46,
        }
    }
}
