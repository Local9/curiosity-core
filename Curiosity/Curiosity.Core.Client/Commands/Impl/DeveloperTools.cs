﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
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
                if (Game.PlayerPed.IsInvincible)
                {
                    Chat.SendLocalMessage("God Mode: Enabled");
                }
                else
                {
                    Chat.SendLocalMessage("God Mode: Disabled");
                }
            }
        }

        [CommandInfo(new[] { "weapons" })]
        public class Weapons : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Enum.GetValues(typeof(WeaponHash)).Cast<WeaponHash>().ToList().ForEach(w =>
                {
                    Game.PlayerPed.Weapons.Give(w, 999, false, true);
                    Game.PlayerPed.Weapons[w].InfiniteAmmo = true;
                    Game.PlayerPed.Weapons[w].InfiniteAmmoClip = true;
                });

                Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed);
                Chat.SendLocalMessage("Weapons: All Equiped");
            }
        }
        #endregion

        #region Vehicles
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

                        API.SetVehicleNitroEnabled(Cache.PersonalVehicle.Handle, true);
                        return;
                    }

                    var model = new Model(API.GetHashKey(argument));

                    if (!model.IsValid || !model.IsVehicle) return;

                    if (Cache.PersonalVehicle != null)
                    {
                        EventSystem.GetModule().Send("entity:delete", Cache.PersonalVehicle.NetworkId);

                        while (Cache.PersonalVehicle.Exists())
                        {
                            await BaseScript.Delay(100);
                        }
                    }

                    var position = entity.Position;
                    var vehicle = await World.CreateVehicle(model, position.AsVector(), position.Heading);

                    player.User.Send("vehicle:log:player", vehicle.NetworkId);
                    Cache.PersonalVehicle = vehicle;

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

        [CommandInfo(new[] { "dv", "deleteveh" })]
        public class VehicleDespawner : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (entity.Vehicle != null)
                {
                    entity.Vehicle?.Delete();
                }
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

                Vector3 pos = Game.PlayerPed.Position;

                bool response = await EventSystem.GetModule().Request<bool>("developer:savePos", positionName, pos.X, pos.Y, pos.Z, Game.PlayerPed.Heading);
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
                        break;
                    default:
                        Chat.SendLocalMessage($"Argument '{argument}' is not implemented.");
                        break;
                }
            }
        }

        //[CommandInfo(new[] { "notify" })]
        //public class NotificationTest : ICommand
        //{
        //    public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
        //    {
        //        if (arguments.Count <= 0) return;

        //        string argument = arguments[0];

        //        string notificationKey = "TEST_LABEL";
        //        API.AddTextEntry(notificationKey, "Example Message: ~a~");

        //        //API.BeginTextCommandDisplayText(notificationKey);
        //        //API.AddTextComponentSubstringPlayerName("Hello, World!");
        //        //API.EndTextCommandDisplayText(0.5f, 0.5f);

        //        // string txd = await Game.PlayerPed.GetHeadshot();

        //        string txd = "CHAR_ACTING_UP";

        //        API.BeginTextCommandDisplayText("STRING");
        //        API.EndTextCommandThefeedPostMessagetextEntry(txd, txd, false, 0, notificationKey, argument);
        //        API.EndTextCommandThefeedPostTicker(true, false);

        //        API.UnregisterPedheadshot(Game.PlayerPed.Handle);
        //    }
        //}
        #endregion
    }
}
