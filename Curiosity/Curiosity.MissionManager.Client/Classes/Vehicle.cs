using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.NaturalMotion;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Classes
{
    [Serializable] // WORK on entity inheritance
    public class Vehicle : Entity, IEquatable<Vehicle>
    {
        private bool _canExecuteAnimation = true;
        private bool _doAnimBonnetOpen = false;
        private bool _doAnimBonnetClose = false;
        private bool _doAnimBootOpen = false;
        private bool _doAnimBootClose = false;

        private int _counterBonnetOpen = 0;
        private int _counterBonnetClose = 0;
        private int _counterBootOpen = 0;
        private int _counterBootClose = 0;

        public CitizenFX.Core.Vehicle Fx { get; private set; }
        public Vector3 Position => Fx.Position;
        public string Hash => Fx.Model.ToString();

        private PluginManager PluginInstance => PluginManager.Instance;

        public string Name => API.GetLabelText(API.GetDisplayNameFromVehicleModel((uint)Fx.Model.Hash));

        public bool IsTowable
        {
            get
            {
                return Decorators.GetBoolean(Fx.Handle, Decorators.VEHICLE_TOW);
            }
            set
            {
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_TOW, value);
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
                Decorators.Set(Fx.Handle, Decorators.VEHICLE_MISSION, value);
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

        private Vehicle(CitizenFX.Core.Vehicle fx) : base(fx.Handle)
        {
            Fx = fx;
        }

        public static async Task<Vehicle> Spawn(Model model, Vector3 position)
        {
            Vector3 streetPosition = position.Street();
            CitizenFX.Core.Vehicle fxVehicle = await World.CreateVehicle(model, streetPosition);
            
            API.ClearAreaOfEverything(streetPosition.X, streetPosition.Y, streetPosition.Z, 5f, false, false, false, false);

            Logger.Debug(fxVehicle.ToString());

            API.NetworkFadeInEntity(fxVehicle.Handle, false);

            return new Vehicle(fxVehicle);
        }

        internal async void Dismiss()
        {
            Fx.IsPersistent = false;
            API.NetworkFadeOutEntity(base.Handle, false, false);
            await BaseScript.Delay(2000);
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

        internal void Impound()
        {
            // ImpoundManager.Tow(this);
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
