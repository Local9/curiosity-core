using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.ClientEvents;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
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
        public DateTime DateCreated;

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
                var _value = Fx.State.Get(StateBagKey.VEHICLE_IMPORTANT);

                if (_value == null)
                {
                    Fx.State.Set(StateBagKey.VEHICLE_IMPORTANT, false, true);
                    return false;
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                Fx.State.Set(StateBagKey.VEHICLE_IMPORTANT, value, true);
            }
        }

        public bool IsTowable
        {
            get
            {
                var _value = Fx.State.Get(StateBagKey.VEHICLE_TOW);

                if (_value == null)
                {
                    Fx.State.Set(StateBagKey.VEHICLE_TOW, false, true);
                    return false;
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                Fx.State.Set(StateBagKey.VEHICLE_TOW, value, true);

                EventSystem.Send("mission:update:vehicle:towable", Fx.NetworkId, true);
            }
        }

        public bool IsSearchable
        {
            get
            {
                var _value = Fx.State.Get(StateBagKey.VEHICLE_SEARCH);

                if (_value == null)
                {
                    Fx.State.Set(StateBagKey.VEHICLE_SEARCH, false, true);
                    return false;
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                Fx.State.Set(StateBagKey.VEHICLE_SEARCH, value, true);
            }
        }

        public bool IsMission
        {
            get
            {
                var _value = Fx.State.Get(StateBagKey.VEHICLE_MISSION);

                if (_value == null)
                {
                    Fx.State.Set(StateBagKey.VEHICLE_MISSION, false, true);
                    return false;
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                Fx.IsPersistent = value;
                Fx.State.Set(StateBagKey.VEHICLE_MISSION, value, true);

                EventSystem.Send("mission:update:vehicle:mission", Fx.NetworkId, true);
            }
        }

        public void RecordLicensePlate()
        {
            EventSystem.Send("mission:update:vehicle:license", Fx.NetworkId, Fx.Mods.LicensePlate, $"{Fx.DisplayName}", $"{Fx.Mods.PrimaryColor}", $"{Fx.Mods.SecondaryColor}");
        }

        public bool IsSpikable
        {
            get
            {
                var _value = Fx.State.Get(StateBagKey.VEHICLE_SPIKE_ALLOWED);

                if (_value == null)
                {
                    Fx.State.Set(StateBagKey.VEHICLE_SPIKE_ALLOWED, false, true);
                    return false;
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                Fx.State.Set(StateBagKey.VEHICLE_SPIKE_ALLOWED, value, true);
            }
        }

        public bool IsIgnored
        {
            get
            {
                var _value = Fx.State.Get(StateBagKey.VEHICLE_TRAFFIC_STOP_IGNORED);

                if (_value == null)
                {
                    Fx.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_IGNORED, false, true);
                    return false;
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                Fx.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_IGNORED, value, true);
            }
        }

        public DateTime LastUpdate { get; private set; }

        public Vehicle(CitizenFX.Core.Vehicle fx, bool update = true) : base(fx.Handle)
        {
            Fx = fx;
            API.NetworkRegisterEntityAsNetworked(fx.Handle);
            API.NetworkRequestControlOfNetworkId(fx.NetworkId);

            DateCreated = DateTime.Now;

            this._eventWrapper = new EntityEventWrapper(this.Fx);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            Fx.State.Set(StateBagKey.VEHICLE_SETUP, true, true);

            if (update)
                EventSystem.Send("mission:add:vehicle", Fx.NetworkId);
        }

        public Blip AttachSuspectBlip()
        {
            if (Fx.AttachedBlip != null) return null;

            API.SetPedAiBlip(Fx.Handle, true);
            API.IsAiBlipAlwaysShown(Fx.Handle, false);
            API.SetAiBlipMaxDistance(Fx.Handle, 50f);
            API.HideSpecialAbilityLockonOperation(Fx.Handle, false);
            API.SetAiBlipType(Fx.Handle, 0);

            Blip blip = Fx.AttachBlip();
            blip.Color = BlipColor.Red;
            blip.Alpha = 255;
            blip.Scale = .75f;

            EventSystem.Send("mission:update:vehicle:blip", Fx.NetworkId, true);

            return blip;
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

            EventSystem.Send("delete:entity", Fx.NetworkId);

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

            if (this.Position.VDist(Cache.PlayerPed.Position) <= 120f || IsImportant)
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

            if (!Mission.isOnMission && PlayerManager.GetModule().PersonalVehicle != null && !IsIgnored && Fx.Driver.Exists() && IsValidVehicle())
            {
                if (Cache.PlayerPed.CurrentVehicle == PlayerManager.GetModule().PersonalVehicle && Cache.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Emergency && Cache.PlayerPed.IsInVehicle())
                {
                    bool isMarked = Fx.State.Get(StateBagKey.VEHICLE_TRAFFIC_STOP_MARKED) ?? false;

                    if (Utility.RANDOM.Bool(0.10f) && !isMarked && Fx.Driver.Exists())
                    {
                        Fx.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_PULLOVER, true, true);
                        AttachSuspectBlip();
                    }

                    Fx.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_MARKED, true, true);

                    CitizenFX.Core.Vehicle playerVeh = PlayerManager.GetModule().PersonalVehicle;

                    bool isPullover = Fx.State.Get(StateBagKey.VEHICLE_TRAFFIC_STOP_PULLOVER) ?? false;

                    if (playerVeh.GetVehicleInFront(10f, 1f) == this.Fx && Fx.Driver != null && TrafficStopManager.Manager.tsVehicle == null && isPullover)
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
                                Cache.PlayerPed.CurrentVehicle.IsSirenActive = i % 2 == 0;
                                await BaseScript.Delay(100);
                            }
                            Cache.PlayerPed.CurrentVehicle.IsSirenActive = false;
                        }

                        if (ControlHelper.IsControlJustPressed(Control.Cover, false))
                            IsIgnored = true;
                    }
                }
            }

            bool isVehDebug = Cache.PlayerPed.State.Get(StateBagKey.PLAYER_DEBUG_VEH) ?? false;

            if (isVehDebug && !_DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                Instance.AttachTickHandler(OnDeveloperOverlay);
                _DEBUG_ENABLED = true;
            }
            else if (!isVehDebug && _DEBUG_ENABLED && Cache.Player.User.IsDeveloper)
            {
                _DEBUG_ENABLED = false;
                Instance.DetachTickHandler(OnDeveloperOverlay);
            }
        }

        private bool IsValidVehicle()
        {
            if (Fx.Driver.IsPlayer) return false;

            if (Fx.Driver.IsDead) return false;

            return Fx.ClassType.Equals(VehicleClass.Compacts)
                || Fx.ClassType.Equals(VehicleClass.Coupes)
                || Fx.ClassType.Equals(VehicleClass.Motorcycles)
                || Fx.ClassType.Equals(VehicleClass.Muscle)
                || Fx.ClassType.Equals(VehicleClass.OffRoad)
                || Fx.ClassType.Equals(VehicleClass.Sedans)
                || Fx.ClassType.Equals(VehicleClass.Sports)
                || Fx.ClassType.Equals(VehicleClass.SportsClassics)
                || Fx.ClassType.Equals(VehicleClass.Super)
                || Fx.ClassType.Equals(VehicleClass.SUVs)
                || Fx.ClassType.Equals(VehicleClass.Vans);
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
                            float single = Vector3.Distance(worldPositioinOfBone, Cache.PlayerPed.Position);
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
                            Cache.PlayerPed.Heading = Fx.Heading - 90f;
                            Cache.PlayerPed.Task.PlayAnimation("veh@low@front_ds@enter_exit", "d_open_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.FrontLeftDoor].Open();
                            await BaseScript.Delay(500);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Cache.PlayerPed.Heading = Fx.Heading;
                            Cache.PlayerPed.Task.PlayAnimation("veh@low@front_ds@enter_exit", "d_close_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.FrontLeftDoor].Close();
                        }
                    }
                    else if (closestDoor == "handle_dside_r")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 2) < 0.25f)
                        {
                            Cache.PlayerPed.Heading = Fx.Heading - 90f;
                            Cache.PlayerPed.Task.PlayAnimation("veh@low@front_ds@enter_exit", "d_open_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.BackLeftDoor].Open();
                            await BaseScript.Delay(500);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Cache.PlayerPed.Heading = Fx.Heading;
                            Cache.PlayerPed.Task.PlayAnimation("veh@low@front_ds@enter_exit", "d_close_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.BackLeftDoor].Close();
                        }
                    }
                    else if (closestDoor == "handle_pside_f")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 1) < 0.25f)
                        {
                            Cache.PlayerPed.Heading = Fx.Heading + 90f;
                            Cache.PlayerPed.Task.PlayAnimation("veh@low@front_ps@enter_exit", "d_open_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.FrontRightDoor].Open();
                            await BaseScript.Delay(500);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Cache.PlayerPed.Heading = Fx.Heading;
                            Cache.PlayerPed.Task.PlayAnimation("veh@low@front_ps@enter_exit", "d_close_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.FrontRightDoor].Close();
                        }
                    }
                    else if (closestDoor == "handle_pside_r")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 3) < 0.25f)
                        {
                            Cache.PlayerPed.Heading = Fx.Heading + 90f;
                            Cache.PlayerPed.Task.PlayAnimation("veh@low@front_ps@enter_exit", "d_open_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.BackRightDoor].Open();
                            await BaseScript.Delay(500);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Cache.PlayerPed.Heading = Fx.Heading;
                            Cache.PlayerPed.Task.PlayAnimation("veh@low@front_ps@enter_exit", "d_close_out", 4f, -1, AnimationFlags.None);
                            Fx.Doors[VehicleDoorIndex.BackRightDoor].Close();
                        }
                    }
                    else if (closestDoor == "bonnet")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 4) < 0.25f)
                        {
                            Cache.PlayerPed.Heading = Fx.Heading - 180f;
                            Cache.PlayerPed.Task.PlayAnimation("rcmnigel3_trunk", "out_trunk_trevor", 4f, 2500, AnimationFlags.None);
                            await BaseScript.Delay(750);
                            Fx.Doors[VehicleDoorIndex.Hood].Open();
                            await BaseScript.Delay(1000);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Cache.PlayerPed.Heading = Fx.Heading - 180f;
                            Cache.PlayerPed.Task.PlayAnimation("rcmepsilonism8", "bag_handler_close_trunk_walk_left", 4f, 2500, AnimationFlags.None);
                            await BaseScript.Delay(1250);
                            Fx.Doors[VehicleDoorIndex.Hood].Close();
                        }
                    }
                    else if (closestDoor == "boot")
                    {
                        if (API.GetVehicleDoorAngleRatio(Handle, 5) < 0.25f)
                        {
                            Cache.PlayerPed.Heading = Fx.Heading;
                            Cache.PlayerPed.Task.PlayAnimation("rcmnigel3_trunk", "out_trunk_trevor", 4f, 2500, AnimationFlags.None);
                            await BaseScript.Delay(750);
                            Fx.Doors[VehicleDoorIndex.Trunk].Open();
                            await BaseScript.Delay(1000);
                            AnimationHandler.AnimationSearch();
                        }
                        else
                        {
                            Cache.PlayerPed.Heading = Fx.Heading;
                            Cache.PlayerPed.Task.PlayAnimation("rcmepsilonism8", "bag_handler_close_trunk_walk_left", 4f, 2500, AnimationFlags.None);
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
