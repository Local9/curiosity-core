﻿using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Exceptions;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Interface.Menus.VehicleMods;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Managers.UI;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Commands.Impl
{
    public class DeveloperTools : CommandContext
    {
        public override string[] Aliases { get; set; } = { "dev", "developer", "d" };
        public override string Title { get; set; } = "Developer";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER, Role.PROJECT_MANAGER };

        static Vector3 positionSave = Vector3.Zero;
        static List<Ped> companions = new List<Ped>();
        static Scaleform testScaleform;

        static string musicEvent = string.Empty;

        static NotificationManager notification => NotificationManager.GetModule();

        #region Weapons
        [CommandInfo(new[] { "weapons" })]
        public class Weapons : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    Enum.GetValues(typeof(WeaponHash)).Cast<WeaponHash>().ToList().ForEach(w =>
                    {
                        Cache.PlayerPed.Weapons.Give(w, 999, false, true);
                        Cache.PlayerPed.Weapons[w].InfiniteAmmo = true;
                        Cache.PlayerPed.Weapons[w].InfiniteAmmoClip = true;
                    });

                    Cache.PlayerPed.Weapons.Select(WeaponHash.Unarmed);
                }

                if (arguments.Count == 1)
                {
                    int weapon = API.GetHashKey(arguments.ElementAt(0));
                    if (weapon > 0)
                    {
                        API.GiveWeaponToPed(Game.PlayerPed.Handle, (uint)weapon, 999, false, true);
                    }
                    else
                    {
                        Cache.PlayerPed.Weapons.Give((WeaponHash)weapon, 999, false, true);
                    }
                }
            }
        }
        #endregion

        #region Player

        [CommandInfo(new[] { "e" })]
        public class ScenarioTesting : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Game.PlayerPed.Task.ClearAllImmediately();

                Game.PlayerPed.Task.PlayAnimation(arguments[0], arguments[1]);
            }
        }

        [CommandInfo(new[] { "eui" })]
        public class EntityOverlay : ICommand
        {
            bool toggle = false;

            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                toggle = !toggle;

                if (toggle)
                    PluginManager.Instance.AttachTickHandler(OnDrawObjectData);

                if (!toggle)
                    PluginManager.Instance.DetachTickHandler(OnDrawObjectData);

                string msg = toggle ? "ENABLED" : "DISABLED";
                notification.Info($"EUI: {msg}");
            }

            private async Task OnDrawObjectData()
            {
                Prop[] entities = World.GetAllProps();

                foreach (Prop prop in entities)
                {
                    if (!prop.IsInRangeOf(Cache.PlayerPed.Position, 3f)) continue;

                    NativeUI.Notifications.ShowFloatingHelpNotification("This Prop", prop.Position);
                    DebugDisplay.DrawData(prop);
                }
            }
        }

        [CommandInfo(new[] { "party" })]
        public class PartyEvent : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                float radius = API.GetRandomFloatInRange(5f, 10f);
                int speakers = API.GetRandomIntInRange(2, 7);
                int peds = API.GetRandomIntInRange(10, 20);
                int booze = API.GetRandomIntInRange(10, 20);

                var center = API.GetEntityCoords(API.GetPlayerPed(-1), true);
                var station = API.GetPlayerRadioStationIndex();

                if (station == 255) // OFF
                    station = 19; // RADIO_19_USER

                for (int i = 0; i < speakers; ++i)
                {
                    var model = Props.SpeakerList[API.GetRandomIntInRange(0, Props.SpeakerList.Length)];
                    var prop = await Props.SpawnInRange(center, model, 1f, radius, false);
                    Props.SetSpeaker(prop, station);
                }

                for (int i = 0; i < peds; ++i)
                {
                    List<string> cPeds = ConfigurationManager.GetModule().PartyPeds();

                    uint[] partyPeds = new uint[cPeds.Count];

                    for (int x = 0; x < cPeds.Count; x++)
                    {
                        int hash = API.GetHashKey(cPeds[i]);
                        partyPeds[x] = (uint)hash;
                    }

                    var ped = await Peds.SpawnInRange(partyPeds, center, 1f, radius);
                    API.TaskStartScenarioInPlace(ped, "WORLD_HUMAN_PARTYING", 0, true);
                    API.SetPedAsNoLongerNeeded(ref ped);
                }

                for (int i = 0; i < booze; ++i)
                {
                    var model = Props.BoozeList[API.GetRandomIntInRange(0, Props.BoozeList.Length)];
                    var prop = await Props.SpawnInRange(center, model, 1f, radius, false);
                    API.SetEntityAsNoLongerNeeded(ref prop);
                }
            }
        }

        [CommandInfo(new[] { "me" })]
        public class MusicEvent : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (!string.IsNullOrEmpty(musicEvent))
                    CancelMusicEvent(musicEvent);

                musicEvent = arguments.ElementAt<string>(0);
                PrepareMusicEvent(musicEvent);
                await BaseScript.Delay(1000);
                TriggerMusicEvent(musicEvent);
            }
        }

        [CommandInfo(new[] { "fuel" })]
        public class VehicleFuelCommand : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (!Game.PlayerPed.IsInVehicle()) return;
                Vehicle v = Game.PlayerPed.CurrentVehicle;
                v.FuelLevel = float.Parse(arguments[0]);
                v.State.Set(StateBagKey.VEH_FUEL, v.FuelLevel, true);
            }
        }

        [CommandInfo(new[] { "scaleform" })]
        public class ScalefromTest : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (testScaleform is not null)
                {
                    testScaleform.Dispose();
                }

                testScaleform = new Scaleform("scaleform_test");

                int timeout = 1000;
                DateTime start = DateTime.Now;
                while (!testScaleform.IsLoaded && DateTime.Now.Subtract(start).TotalMilliseconds < timeout) await BaseScript.Delay(0);

                Logger.Debug($"Scaleform Loaded: {testScaleform.IsLoaded}");

                while (testScaleform.IsLoaded)
                {
                    testScaleform.Render3D(Cache.PlayerPed.GetOffsetPosition(new Vector3(.05f, .05f, .5f)), GameplayCamera.Rotation, new Vector3(3f, 2f, 3f));
                    await BaseScript.Delay(0);
                }

                testScaleform.Dispose();
            }
        }

        [CommandInfo(new[] { "textEntity" })]
        public class TextEntity : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle veh = Game.PlayerPed.CurrentVehicle;

                Scaleform scaleform = new Scaleform("mp_car_stats_01");

                int timeout = 1000;
                DateTime start = DateTime.Now;
                while (!scaleform.IsLoaded && DateTime.Now.Subtract(start).TotalMilliseconds < timeout) await BaseScript.Delay(0);

                Logger.Debug($"Scaleform Loaded: {scaleform.IsLoaded}");

                string make = GetMakeNameFromVehicleModel((uint)veh.Model.Hash);
                string model = veh.DisplayName;

                uint hash = (uint)veh.Model.Hash;

                float maxSpeed = GetVehicleModelMaxSpeed(hash);
                float maxTraction = GetVehicleModelMaxTraction(hash);
                float accel = GetVehicleModelAcceleration(hash);
                float breaking = GetVehicleModelHandBrake(hash);

                scaleform.CallFunction("SET_VEHICLE_INFOR_AND_STATS",
                                       model,
                                       "Tracked and Registered",
                                       "MPCarHUD",
                                       make,
                                       "Top Speed",
                                       "Accelration",
                                       "Braking",
                                       "Traction",
                                       maxSpeed,
                                       accel,
                                       breaking,
                                       maxTraction);
                //scaleform.CallFunction("setBars", 1, 10);
                //scaleform.CallFunction("setBars", 2, 20);
                //scaleform.CallFunction("setBars", 3, 30);
                //scaleform.CallFunction("setBars", 4, 40);
                //scaleform.CallFunction("debug");

                Logger.Debug($"V: {maxSpeed}, {accel}, {breaking}, {maxTraction}");

                Vector3 min = Vector3.Zero;
                Vector3 max = Vector3.Zero;

                GetModelDimensions((uint)veh.Model.Hash, ref min, ref max);
                Vector3 size = (max - min);

                if (size.Z < 2.5f)
                    size.Z = 2.5f;

                await BaseScript.Delay(100);

                while (scaleform.IsLoaded && veh.Exists())
                {
                    Vector3 vehPos = veh.Position;

                    vehPos.Z = vehPos.Z + size.Z;

                    var rotation = GameplayCamera.Rotation;

                    scaleform.Render3D(vehPos, rotation, new Vector3(4.5f, 3f, 3f));

                    await BaseScript.Delay(0);
                }

                scaleform.Dispose();

            }
        }

        private static Vector3 GameplayCameraForwardVector()
        {
            var rotation = (float)(Math.PI / 180.0) * GameplayCamera.Rotation;
            return Vector3.Normalize(new Vector3((float)-Math.Sin(rotation.Z) * (float)Math.Abs(Math.Cos(rotation.X)), (float)Math.Cos(rotation.Z) * (float)Math.Abs(Math.Cos(rotation.X)), (float)Math.Sin(rotation.X)));
        }

        [CommandInfo(new[] { "scuba" })]
        public class ScubaGearTest : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                PlayerOptionsManager.GetModule().ToggleScubaEquipment();
            }
        }

        [CommandInfo(new[] { "guard" })]
        public class Guard : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                string cmd = arguments.ElementAt(0);

                if (cmd == "remove")
                {
                    List<Ped> peds = new List<Ped>(companions);
                    foreach (Ped ped in peds)
                    {
                        if (ped.Exists())
                        {
                            ped.FadeOut();
                            ped.Delete();
                            companions.Remove(ped);
                        }
                    }
                    return;
                }

                try
                {
                    if (cmd == "add")
                    {
                        // if (companions.Count > 4) return;

                        string pedHash = arguments.ElementAt(1);

                        if (pedHash == "jug")
                        {
                            pedHash = "u_m_y_juggernaut_01";
                        }

                        Model companionModel = await Utilities.LoadModel(pedHash);

                        Vector3 offset = new Vector3(2f, 0f, 0f);
                        Vector3 spawn = Cache.PlayerPed.GetOffsetPosition(offset);
                        float groundZ = spawn.Z;
                        Vector3 groundNormal = Vector3.Zero;

                        if (GetGroundZAndNormalFor_3dCoord(spawn.X, spawn.Y, spawn.Z, ref groundZ, ref groundNormal))
                        {
                            spawn.Z = groundZ;
                        }

                        Ped companionPed = await World.CreatePed(companionModel, spawn, Game.PlayerPed.Heading);
                        companionModel.MarkAsNoLongerNeeded();

                        if (companionPed.IsInGroup)
                            companionPed.LeaveGroup();

                        PedGroup playerPedGroup = Cache.PedGroup;

                        if (playerPedGroup is not null)
                            Logger.Debug($"Current ped group is {playerPedGroup.Handle}");

                        if (playerPedGroup is null)
                            playerPedGroup = new PedGroup();

                        if (!playerPedGroup.Contains(Game.PlayerPed))
                        {
                            playerPedGroup.Add(Game.PlayerPed, true);
                            Logger.Debug($"Added player as group leader");
                        }

                        if (!playerPedGroup.Contains(companionPed))
                        {
                            playerPedGroup.Add(companionPed, false);
                            Logger.Debug($"Added companion as group member");
                        }

                        SetPedToInformRespectedFriends(companionPed.Handle, 20f, 20);
                        SetPedToInformRespectedFriends(Game.PlayerPed.Handle, 20f, 20);

                        companionPed.NeverLeavesGroup = true;
                        companionPed.CanSufferCriticalHits = false;
                        companionPed.Health = companionPed.MaxHealth;
                        companionPed.Armor = 200;
                        companionPed.DropsWeaponsOnDeath = false;

                        companionPed.Accuracy = 100;

                        companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_DieWhenRagdoll, false);
                        companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_DisableHurt, true);
                        companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_DisableShockingEvents, true);
                        companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_IgnoreBeingOnFire, true);
                        companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_IgnoreSeenMelee, true);

                        if (pedHash == "u_m_y_juggernaut_01")
                        {
                            int type = Utility.RANDOM.Next(3);
                            if (type == 0)
                            {
                                SetPedPropIndex(companionPed.Handle, 0, 0, 0, false);
                                SetPedComponentVariation(companionPed.Handle, 0, 0, 1, 0);
                                SetPedComponentVariation(companionPed.Handle, 3, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 4, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 5, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 8, 0, 1, 0);
                                SetPedComponentVariation(companionPed.Handle, 10, 0, 1, 0);
                            }
                            else if (type == 1)
                            {
                                SetPedPropIndex(companionPed.Handle, 0, 0, 0, false);
                                SetPedComponentVariation(companionPed.Handle, 0, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 3, 0, 1, 0);
                                SetPedComponentVariation(companionPed.Handle, 4, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 5, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 8, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 10, 0, 0, 0);
                            }
                            else
                            {
                                ClearPedProp(companionPed.Handle, 0);
                                SetPedComponentVariation(companionPed.Handle, 0, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 3, 0, 1, 0);
                                SetPedComponentVariation(companionPed.Handle, 4, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 5, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 8, 0, 0, 0);
                                SetPedComponentVariation(companionPed.Handle, 10, 0, 0, 0);
                            }

                            companionPed.Weapons.Give(WeaponHash.Minigun, 999, false, true);
                            companionPed.Health = 5000;
                            companionPed.CanRagdoll = false;
                            companionPed.IsMeleeProof = true;
                            companionPed.FiringPattern = FiringPattern.FullAuto;
                        }
                        else
                        {
                            companionPed.Weapons.Give(WeaponHash.AdvancedRifle, 999, false, true);
                        }

                        companions.Add(companionPed);

                        PluginManager.Instance.AttachTickHandler(OnCleanUpGuards);
                    }
                }
                catch (CitizenFxException cfxEx)
                {
                    NotificationManager.GetModule().Error(cfxEx.Message);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Create Companion");
                }
            }
        }

        private static async Task OnCleanUpGuards()
        {
            if (companions.Count == 0)
            {
                PluginManager.Instance.DetachTickHandler(OnCleanUpGuards);
            }

            List<Ped> copy = new List<Ped>(companions);

            foreach (Ped ped in copy)
            {
                if (ped is not null)
                {
                    if (ped.Exists())
                    {
                        if (ped.IsDead)
                        {
                            await ped.FadeOut();
                            ped.Delete();
                            companions.Remove(ped);
                        }
                    }

                    if (!ped.Exists())
                    {
                        companions.Remove(ped);
                    }
                }
            }
        }

        static string helpText = string.Empty;
        static Vector3 helpTextPosition;

        [CommandInfo(new[] { "txtHelp" })]
        public class HelpTextTest : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                string cmd = arguments.ElementAt(0);

                if (cmd == "add")
                {
                    helpTextPosition = Game.PlayerPed.Position;
                    helpText = arguments.ElementAt(1);
                    PluginManager.Instance.AttachTickHandler(OnHelpTextDisplay);
                }

                if (cmd == "stop")
                {
                    helpText = string.Empty;
                }
            }
        }

        private static async Task OnHelpTextDisplay()
        {
            if (string.IsNullOrEmpty(helpText))
            {
                PluginManager.Instance.DetachTickHandler(OnHelpTextDisplay);
                return;
            }

            NativeUI.Notifications.ShowFloatingHelpNotification(helpText, helpTextPosition);
        }

        [CommandInfo(new[] { "god" })]
        public class Godmode : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                player.Entity.ToggleGodMode();
                string message = Cache.PlayerPed.IsInvincible ? "God Mode: Enabled" : "God Mode: Disabled";
                Chat.SendLocalMessage(message);
                NotificationManager.GetModule().Info(message);
            }
        }

        [CommandInfo(new[] { "test" })]
        public class TestMode : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                player.Entity.ToggleGodMode();
                string message = Cache.PlayerPed.IsInvincible ? "God Mode: Enabled" : "God Mode: Disabled";
                Chat.SendLocalMessage(message);
                NotificationManager.GetModule().Info(message);

                Enum.GetValues(typeof(WeaponHash)).Cast<WeaponHash>().ToList().ForEach(w =>
                {
                    Cache.PlayerPed.Weapons.Give(w, 999, false, true);
                    Cache.PlayerPed.Weapons[w].InfiniteAmmo = true;
                    Cache.PlayerPed.Weapons[w].InfiniteAmmoClip = true;
                });
            }
        }

        [CommandInfo(new[] { "yeet" })]
        public class Yeet : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Entity ent = Game.PlayerPed.GetEntityInFront();

                float randX = PluginManager.Rand.Next(3, 40);
                float randY = PluginManager.Rand.Next(3, 40);
                float randZ = PluginManager.Rand.Next(100, 400);

                ent.Velocity = new Vector3(randX, randY, randZ);
            }
        }

        [CommandInfo(new[] { "stat" })]
        public class StatNotifcation : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                NativeUI.Notifications.ShowStatNotification(10, 9, "Stat Test");
            }
        }

        [CommandInfo(new[] { "mod" })]
        public class ModVehicleMenu : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                VehicleModMenu.GetModule().OpenMenu();
            }
        }

        [CommandInfo(new[] { "anim" })]
        public class Animation : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                string dict = arguments.ElementAt(0);
                string anim = arguments.ElementAt(1);

                Cache.PlayerPed.Task.ClearAllImmediately();
                Cache.PlayerPed.Task.ClearAll();

                API.RequestAnimDict(dict);
                while (!API.HasAnimDictLoaded(dict))
                {
                    await BaseScript.Delay(0);
                }

                Enum.TryParse(arguments.ElementAt(2), out AnimationFlags animationFlag);

                Cache.PlayerPed.Task.PlayAnimation(dict, anim, 8f, -1, animationFlag);
            }
        }
        #endregion

        #region Vehicles
        [CommandInfo(new[] { "train" })]
        public class Train : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                try
                {
                    if (arguments.Count <= 0) return;
                    int trainId = int.Parse(arguments.ElementAt(0));
                    bool state = int.Parse(arguments.ElementAt(1)) == 1;
                    API.SwitchTrainTrack(trainId, state);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Train -> {ex.Message}");
                }
            }
        }

        [CommandInfo(new[] { "vn", "vehicleNet" })]
        public class VehicleDespawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = Game.PlayerPed.GetVehicleInFront();
                Notify.Info($"~n~NetId: {vehicle.NetworkId}~n~Hndl: {vehicle.Handle}");
            }
        }

        [CommandInfo(new[] { "els" })]
        public class ElsSpawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                try
                {
                    if (arguments.Count <= 0) return;

                    var model = new Model(API.GetHashKey(arguments.ElementAt(0)));

                    if (!model.IsValid || !model.IsVehicle) return;

                    var position = entity.Position;
                    var vehicle = PluginManager.Instance.ExportDictionary["elsplus"].SpawnCar(arguments.ElementAt(0));

                    entity.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                }
                catch (Exception)
                {
                    // Ignored
                }
            }
        }

        [CommandInfo(new[] { "repair", "fix", "wash" })]
        public class VehicleRepairer : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                var vehicle = entity.Vehicle;

                if (vehicle == null) return;

                vehicle.Wash();
                vehicle.Repair();
                vehicle.PlaceOnGround();
            }
        }
        #endregion

        #region Positions
        [CommandInfo(new[] { "tuner" })]
        public class Tuner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vector3 position = new Vector3(-2134, 1106, -27);

                API.RequestIpl("tr_tuner_meetup");
                API.RequestIpl("tr_tuner_race_line");

                int interiorId = API.GetInteriorAtCoords(-2000.0f, 1113.211f, -25.36243f);

                if (API.IsValidInterior(interiorId))
                    API.RefreshInterior(interiorId);

                // API.ActivateInteriorEntitySet(interiorId, "entity_set_meet_crew");
                API.ActivateInteriorEntitySet(interiorId, "entity_set_meet_lights");
                API.ActivateInteriorEntitySet(interiorId, "entity_set_meet_lights_cheap");
                // API.ActivateInteriorEntitySet(interiorId, "entity_set_player"); // ugly flag
                // API.ActivateInteriorEntitySet(interiorId, "entity_set_test_crew");
                API.ActivateInteriorEntitySet(interiorId, "entity_set_test_lights");
                API.ActivateInteriorEntitySet(interiorId, "entity_set_test_lights_cheap");
                API.ActivateInteriorEntitySet(interiorId, "entity_set_time_trial");

                if (API.IsValidInterior(interiorId))
                    API.RefreshInterior(interiorId);

                // position.Z = World.GetGroundHeight(position) + 2;

                if (Game.PlayerPed.IsInVehicle())
                {
                    API.SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, position.X, position.Y, position.Z);
                    return;
                }

                await player.Entity.Teleport(position.ToPosition());
            }
        }

        [CommandInfo(new[] { "tpm" })]
        public class TeleportMarker : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                var waypoint = World.GetWaypointBlip();

                if (waypoint == null) return;

                await ScreenInterface.FadeOut();
                player.Entity.CitizenPed.IsInvincible = true;
                var wp = waypoint.Position;

                Vector3 position = new Vector3(wp.X, wp.Y, wp.Z);

                float ground = 0f;
                if (API.GetGroundZFor_3dCoord_2(position.X, position.Y, position.Z, ref ground, false))
                    position.Z = ground;

                if (Game.PlayerPed.IsInVehicle())
                {
                    API.SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, position.X, position.Y, position.Z);
                    goto FADEIN;
                }

                await player.Entity.Teleport(position.ToPosition());

                await BaseScript.Delay(2000);

                ground = 0f;
                position.Z = World.GetGroundHeight(new Vector2(position.X, position.Y));

                if (API.GetGroundZFor_3dCoord_2(position.X, position.Y, position.Z, ref ground, false))
                    position.Z = ground;

                if (Game.PlayerPed.IsInVehicle())
                {
                    API.SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, position.X, position.Y, position.Z);
                    goto FADEIN;
                }

                await player.Entity.Teleport(position.ToPosition());

            FADEIN:
                player.Entity.CitizenPed.IsInvincible = false;
                await ScreenInterface.FadeIn();
            }
        }

        [CommandInfo(new[] { "tp", "coords" })]
        public class TeleportCoords : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count < 3) return;

                try
                {
                    string xStr = arguments[0];
                    string yStr = arguments[1];
                    string zStr = arguments[2];

                    xStr = xStr.Replace(",", "").Replace("f", "");
                    yStr = yStr.Replace(",", "").Replace("f", "");
                    zStr = zStr.Replace(",", "").Replace("f", "");

                    float x = float.Parse(xStr);
                    float y = float.Parse(yStr);
                    float z = float.Parse(zStr);

                    var position = new Vector3(x, y, z);

                    if (Game.PlayerPed.IsInVehicle())
                    {
                        API.SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, position.X, position.Y, position.Z);
                        return;
                    }

                    await player.Entity.Teleport(position.ToPosition());
                }
                catch (Exception ex)
                {
                    Chat.SendLocalMessage("Invalid or Missing Coord");
                }

            }
        }

        [CommandInfo(new[] { "blip" })]
        public class BlipTest : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Blip blip = World.CreateBlip(Game.PlayerPed.Position);
                blip.Sprite = (BlipSprite)int.Parse(arguments.ElementAt(0));
                blip.IsShortRange = true;
                API.SetBlipShrink(blip.Handle, true);
            }
        }

        [CommandInfo(new[] { "pos" })]
        public class SaveCoords : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count <= 0) return;

                string positionName = arguments[0];

                Vector3 pos = Cache.PlayerPed.Position;

                bool response = await EventSystem.GetModule().Request<bool>("developer:savePos", positionName, pos.X, pos.Y, pos.Z, Cache.PlayerPed.Heading);
                if (response)
                {
                    Chat.SendLocalMessage($"Position '{positionName}' saved.");
                }
                else
                {
                    Chat.SendLocalMessage($"Issue when trying to save position: {positionName}.");
                }
            }
        }

        [CommandInfo(new[] { "rr" })]
        public class Refresh : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count <= 0) return;

                string argument = arguments[0];

                switch (argument)
                {
                    case "locations":
                    case "loc":
                        LocationManager.LocationManagerInstance.OnGetLocations();
                        NotificationManager.GetModule().Success($"Refreshed locations.");
                        EventSystem.GetModule().Send("config:locations");
                        break;
                    default:
                        Chat.SendLocalMessage($"Argument '{argument}' is not implemented.");
                        break;
                }
            }
        }

        [CommandInfo(new[] { "health" })]
        public class Health : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count <= 0) return;

                int health = 100;
                if (int.TryParse(arguments[0], out health))
                {
                    Game.PlayerPed.Health = health;
                    NotificationManager.GetModule().Success($"Health set to {health}");
                }
            }
        }

        [CommandInfo(new[] { "notification" })]
        public class NotificationTest : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                string message = "";
                message += "Fishing Mode has been activated<br>Fishing Mode has been activated<br>Fishing Mode has been activated<br>Fishing Mode has been activated<br>";

                NotificationManager.GetModule().Success(message, "bottom-right");
            }
        }

        #endregion

        #region Screen
        [CommandInfo(new[] { "ui" })]
        public class UserInterface : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                DeveloperUxManager developerUxManager = DeveloperUxManager.GetModule();

                if (arguments.Count > 0)
                {
                    if (arguments[0] == "scale" || arguments[0] == "s")
                        developerUxManager.Scale = float.Parse(arguments[1]);
                }

                if (arguments.Count == 0)
                {
                    if (developerUxManager.Enabled)
                    {
                        developerUxManager.DisableDeveloperOverlay();
                        return;
                    }

                    if (!developerUxManager.Enabled)
                    {
                        developerUxManager.EnableDeveloperOverlay();
                        return;
                    }
                }
            }
        }
        #endregion

        #region thirdParty
        [CommandInfo(new[] { "song" })]
        public class WorldSong : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    Chat.SendLocalMessage($"Missing url.");
                    return;
                }

                string url = arguments[0];
                float volume = arguments.Count == 2 ? float.Parse(arguments[1]) : 0.5f;

                PluginManager pluginManager = PluginManager.Instance;

                Vector3 pos = Game.PlayerPed.Position;

                pluginManager.ExportDictionary["xsound"].PlayUrlPos("devSong", url, volume, pos, false);
            }
        }
        #endregion

        [CommandInfo(new[] { "distance" })]
        public class Distance : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    Chat.SendLocalMessage($"Missing position index.");
                    return;
                }

                if (arguments[0] == $"{1}")
                {
                    positionSave = Game.PlayerPed.Position;
                }
                else
                {
                    if (positionSave != Vector3.Zero)
                    {
                        Logger.Debug($"{positionSave.Distance(Game.PlayerPed.Position)}");
                    }
                }
            }
        }

        [CommandInfo(new[] { "lock" })]
        public class LockTesting : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (!Game.PlayerPed.IsInVehicle())
                {
                    Chat.SendLocalMessage($"Need to be in a vehicle.", "help");
                    return;
                }

                try
                {
                    int toggle = int.Parse(arguments.ElementAt(0));
                    Vehicle veh = Game.PlayerPed.CurrentVehicle;

                    if (toggle == 0)
                    {
                        API.SetVehicleExclusiveDriver_2(veh.Handle, 0, 0);
                    }
                    else
                    {
                        API.SetVehicleExclusiveDriver_2(veh.Handle, Game.PlayerPed.Handle, toggle);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "DevCmd: Lock");
                }
            }
        }

        [CommandInfo(new[] { "car" })]
        public class DevCar : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count == 1)
                {
                    await SpawnVehicle(arguments);
                    return;
                }

                int.TryParse(arguments.ElementAt(1), out int numberOfVehicles);
                if (numberOfVehicles > 0)
                {
                    while (numberOfVehicles > 0)
                    {
                        await SpawnVehicle(arguments);
                        numberOfVehicles--;
                        await BaseScript.Delay(100);
                    }

                    Notify.Alert($"Finished spawning.");
                }
            }

            private static async Task SpawnVehicle(List<string> arguments)
            {
                try
                {
                    Vehicle vehicle = null;

                    if (Game.PlayerPed.IsInVehicle())
                    {
                        Vehicle del = Game.PlayerPed.CurrentVehicle;
                        Game.PlayerPed.Task.WarpOutOfVehicle(del);
                        await BaseScript.Delay(100);

                        EventSystem.GetModule().Send("delete:entity", del.NetworkId);
                        del.Delete();
                    }

                    Model vehModel = new Model(arguments.ElementAt(0));

                    if (!API.IsModelInCdimage((uint)vehModel.Hash))
                    {
                        NotificationManager.GetModule().Error($"Model '{arguments.ElementAt(0)}' is not loaded.");
                        return;
                    }

                    if (!vehModel.IsValid)
                    {
                        NotificationManager.GetModule().Error($"Model '{arguments.ElementAt(0)}' is not valid.");
                        return;
                    }

                    DateTime maxTime = DateTime.UtcNow.AddSeconds(10);

                    while (!vehModel.IsLoaded)
                    {
                        await vehModel.Request(3000);

                        if (DateTime.UtcNow > maxTime) break;
                    }

                    if (!vehModel.IsLoaded)
                    {
                        NotificationManager.GetModule().Error("Vehicle was unable to load.<br>If the vehicle is a custom model, please try again after it has finished downloading.");
                        return;
                    }

                    if (arguments.Count >= 1 && arguments.Count < 5)
                    {
                        Vector3 pos = Game.PlayerPed.Position;
                        pos.Z = pos.Z - 50f;

                        vehicle = await World.CreateVehicle(vehModel, pos, Game.PlayerPed.Heading);
                    }

                    if (arguments.Count == 5)
                    {
                        float x = float.Parse(arguments.ElementAt(1));
                        float y = float.Parse(arguments.ElementAt(2));
                        float z = float.Parse(arguments.ElementAt(3));
                        float h = float.Parse(arguments.ElementAt(4));

                        Vector3 pos = new Vector3(x, y, z);

                        vehicle = await World.CreateVehicle(vehModel, pos, h);
                    }

                    vehicle.IsPersistent = true;
                    vehicle.PreviouslyOwnedByPlayer = true;
                    vehicle.IsPositionFrozen = true;
                    vehicle.IsCollisionEnabled = false;

                    await vehicle.FadeOut();
                    vehicle.Repair();

                    await BaseScript.Delay(500);

                    if (vehModel?.IsLoaded ?? false)
                        vehModel.MarkAsNoLongerNeeded();

                    API.NetworkRequestControlOfEntity(vehicle.Handle);
                    API.SetNetworkIdExistsOnAllMachines(vehicle.NetworkId, true);
                    API.SetNetworkIdCanMigrate(vehicle.NetworkId, true);
                    API.SetVehicleHasBeenOwnedByPlayer(vehicle.Handle, true);

                    bool b = await EventSystem.GetModule().Request<bool>("garage:set:vehicle", vehicle.NetworkId, (int)SpawnType.Vehicle);

                    if (b)
                    {
                        vehicle.CreateBlip();
                        Cache.StaffVehicle = new State.VehicleState(vehicle);

                        if (arguments.Count == 1)
                            Cache.PlayerPed.Task.WarpIntoVehicle(Cache.StaffVehicle.Vehicle, VehicleSeat.Driver);

                        Cache.Player.User.SendEvent("vehicle:log:staff", vehicle.NetworkId);

                        vehicle.IsPositionFrozen = false;
                        vehicle.IsCollisionEnabled = true;

                        vehicle.Position = Common.GetRandomSpawnCoordsInRange(Game.PlayerPed.Position, 10f, 25f, out float heading, false);
                        vehicle.Heading = heading;

                        vehicle.FadeIn();
                        vehicle.Opacity = 255;
                        vehicle.ResetOpacity();

                        vehicle.Mods.InstallModKit();
                        SetVehicleMod(vehicle.Handle, 11, 5, false);
                        SetVehicleMod(vehicle.Handle, 12, 5, false);
                        SetVehicleMod(vehicle.Handle, 13, 5, false);
                        SetVehicleMod(vehicle.Handle, 15, 5, false);
                        SetVehicleMod(vehicle.Handle, 18, 1, false);

                        return;
                    }

                    EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);
                    vehicle.Delete();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Vehicle Spawn");
                }
            }
        }
    }
}
