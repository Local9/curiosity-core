using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Interface.Menus.VehicleMods;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Managers.UI;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Curiosity.Core.Client.Exceptions;

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

        #region Player
        [CommandInfo(new[] { "guard" })]
        public class Guard : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                string cmd = arguments.ElementAt(0);

                if (cmd == "remove")
                {
                    foreach(Ped ped in companions)
                    {
                        if (ped.Exists())
                        {
                            await ped.FadeOut();
                            ped.Delete();
                        }
                    }
                    return;
                }

                try
                {
                    if (cmd == "add")
                    {
                        if (companions.Count > 4) return;

                        string pedHash = arguments.ElementAt(1);

                        Model companionModel = await Utils.Utility.LoadModel(pedHash);

                        Vector3 offset = new Vector3(2f, 0f, 0f);
                        Vector3 spawn = Cache.PlayerPed.GetOffsetPosition(offset);
                        float groundZ = spawn.Z;
                        Vector3 groundNormal = Vector3.Zero;

                        if (GetGroundZAndNormalFor_3dCoord(spawn.X, spawn.Y, spawn.Z, ref groundZ, ref groundNormal))
                        {
                            spawn.Z = groundZ;
                        }

                        Ped companionPed = await World.CreatePed(companionModel, spawn, Cache.PlayerPed.Heading);
                        companionModel.MarkAsNoLongerNeeded();

                        PedGroup playerGroup = Cache.PlayerPed.PedGroup;
                        playerGroup.FormationType = FormationType.Default;
                        playerGroup.SeparationRange = 2.14748365E+09f; // inifinity

                        playerGroup.Add(Cache.PlayerPed, true);
                        playerGroup.Add(companionPed, false);

                        SetPedToInformRespectedFriends(companionPed.Handle, 20f, 20);
                        SetPedToInformRespectedFriends(Cache.PlayerPed.Handle, 20f, 20);

                        SetGroupFormationSpacing(playerGroup.Handle, 1f, 0.9f, 3f);

                        companionPed.Weapons.Give(WeaponHash.AdvancedRifle, 999, false, true);

                        companions.Add(companionPed);
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

        [CommandInfo(new[] { "god" })]
        public class Godmode : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                player.Entity.ToggleGodMode();
                if (Cache.PlayerPed.IsInvincible)
                {
                    Chat.SendLocalMessage("God Mode: Enabled");
                }
                else
                {
                    Chat.SendLocalMessage("God Mode: Disabled");
                }
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

        [CommandInfo(new[] { "weapons" })]
        public class Weapons : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Enum.GetValues(typeof(WeaponHash)).Cast<WeaponHash>().ToList().ForEach(w =>
                {
                    Cache.PlayerPed.Weapons.Give(w, 999, false, true);
                    Cache.PlayerPed.Weapons[w].InfiniteAmmo = true;
                    Cache.PlayerPed.Weapons[w].InfiniteAmmoClip = true;
                });

                Cache.PlayerPed.Weapons.Select(WeaponHash.Unarmed);
                Chat.SendLocalMessage("Weapons: All Equiped");
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

                var position = waypoint.Position;

                position.Z = World.GetGroundHeight(position) + 2;

                if (Game.PlayerPed.IsInVehicle())
                {
                    API.SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, position.X, position.Y, position.Z);
                    return;
                }

                await player.Entity.Teleport(position.ToPosition());
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
                    float x = float.Parse(arguments[0]);
                    float y = float.Parse(arguments[1]);
                    float z = float.Parse(arguments[2]);

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

                switch(argument)
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
                catch(Exception ex)
                {
                    Logger.Error(ex, "DevCmd: Lock");
                }
            }
        }

        [CommandInfo(new [] { "car" })]
        public class DevCar : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = null;

                if (Cache.Entity is not null && Cache.Entity.Vehicle is not null)
                {
                    if (Cache.Entity.Vehicle.Exists()) // get vehicle player is in
                    {
                        vehicle = Cache.Entity.Vehicle;

                        if (vehicle.Driver == Cache.PlayerPed)
                            Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);

                        await vehicle.FadeOut(true);

                        vehicle.IsPositionFrozen = true;
                        vehicle.IsCollisionEnabled = false;

                        EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);

                        if (vehicle.Exists())
                            vehicle.Delete();

                        await BaseScript.Delay(500);
                    }
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

                if (arguments.Count == 1)
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

                    GarageVehicleManager.GetModule().CreateBlip(vehicle);
                    Cache.StaffVehicle = new State.VehicleState(vehicle);
                    Cache.PlayerPed.Task.WarpIntoVehicle(Cache.StaffVehicle.Vehicle, VehicleSeat.Driver);
                    Cache.Player.User.SendEvent("vehicle:log:staff", vehicle.NetworkId);

                    vehicle.IsPositionFrozen = false;
                    vehicle.IsCollisionEnabled = true;

                    vehicle.Position = Game.PlayerPed.Position;

                    await vehicle.FadeIn();
                    vehicle.Opacity = 255;
                    vehicle.ResetOpacity();
                    return;
                }

                EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);
                vehicle.Delete();
            }
        }
    }
}
