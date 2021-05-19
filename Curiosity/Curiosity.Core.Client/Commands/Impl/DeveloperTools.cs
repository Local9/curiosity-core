using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Curiosity.Core.Client.Commands.Impl
{
    public class DeveloperTools : CommandContext
    {
        public override string[] Aliases { get; set; } = { "dev", "developer", "d" };
        public override string Title { get; set; } = "Developer";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER, Role.PROJECT_MANAGER };

        #region Player
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
                Vehicle vehicle = Cache.PlayerPed.GetVehicleInFront();
                Notify.Info($"~n~NetId: {vehicle.NetworkId}~n~Hndl: {vehicle.Handle}");
            }
        }

        [CommandInfo(new[] { "vehicle", "veh", "car" })]
        public class VehicleSpawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                try
                {
                    if (arguments.Count <= 0) return;

                    string argument = arguments.ElementAt(0);

                    if (argument == "nitro")
                    {
                        if (Cache.PersonalVehicle == null) return;

                        API.SetVehicleNitroEnabled(Cache.PersonalVehicle.Vehicle.Handle, true);
                        return;
                    }

                    var model = new Model(API.GetHashKey(argument));

                    if (!model.IsValid || !model.IsVehicle) return;

                    if (Cache.PersonalVehicle != null)
                    {
                        EventSystem.GetModule().Send("entity:delete", Cache.PersonalVehicle.Vehicle.NetworkId);

                        while (Cache.PersonalVehicle.Vehicle.Exists())
                        {
                            await BaseScript.Delay(100);
                            Cache.PersonalVehicle.Vehicle.Delete();
                        }
                    }

                    var position = entity.Position;
                    var vehicle = await World.CreateVehicle(model, position.AsVector(), position.H);

                    player.User.SendEvent("vehicle:log:player", vehicle.NetworkId);
                    Cache.PersonalVehicle = new State.VehicleState(vehicle);

                    entity.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                }
                catch (Exception)
                {
                    // Ignored
                }
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
        [CommandInfo(new[] { "tpm" })]
        public class TeleportMarker : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                var waypoint = World.GetWaypointBlip();

                if (waypoint == null) return;

                var position = waypoint.Position;

                position.Z = World.GetGroundHeight(position) + 2;

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
                        Chat.SendLocalMessage($"Refreshed locations.");
                        EventSystem.GetModule().Send("config:locations");
                        break;
                    default:
                        Chat.SendLocalMessage($"Argument '{argument}' is not implemented.");
                        break;
                }
            }
        }


        #endregion
    }
}
