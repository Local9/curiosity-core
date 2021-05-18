using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Curiosity.Core.Client.Commands.Impl
{
    public class StaffCommands : CommandContext
    {
        static EventSystem EventSystem => EventSystem.GetModule();

        public override string[] Aliases { get; set; } = { "staff", "s" };
        public override string Title { get; set; } = "Staff Commands";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; }
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.ADMINISTRATOR, Role.COMMUNITY_MANAGER, Role.DEVELOPER, Role.HEAD_ADMIN, Role.HELPER, Role.MODERATOR, Role.PROJECT_MANAGER, Role.SENIOR_ADMIN };

        #region Weapons
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

        #region Entities

        [CommandInfo(new[] { "del" })]
        public class EntityDespawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Entity entityToDelete = Cache.PlayerPed.GetEntityInFront();

                if (entityToDelete == null)
                {
                    Notify.Alert($"No entity found.");
                    return;
                }

                API.NetworkRequestControlOfEntity(entityToDelete.Handle);

                while (!API.NetworkHasControlOfEntity(entityToDelete.Handle))
                {
                    await BaseScript.Delay(100);
                    API.NetworkRequestControlOfEntity(entityToDelete.Handle);
                }

                if (entityToDelete is Vehicle)
                {
                    Vehicle veh = (Vehicle)entityToDelete;
                    if (veh.Driver == Cache.PlayerPed)
                        Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);
                }

                await entityToDelete.FadeOut(true);

                EventSystem.GetModule().Send("delete:entity", entityToDelete.NetworkId);

                Entity attached = entityToDelete.GetEntityAttachedTo();

                if (attached is not null)
                {
                    attached.IsCollisionEnabled = false;
                    EventSystem.GetModule().Send("delete:entity", attached.NetworkId);

                    if (attached.Exists())
                        attached.Delete();
                }

                if (entityToDelete.Exists())
                    entityToDelete.Delete();
            }
        }

        [CommandInfo(new[] { "nuke" })]
        public class EntityNukeDespawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                EventSystem.GetModule().Send("entity:nuke");
            }
        }

        #endregion

        #region vehicles
        [CommandInfo(new[] { "dv", "deleteveh" })]
        public class VehicleDespawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = null;

                if (entity.Vehicle != null)
                {
                    vehicle = entity.Vehicle;
                }
                else
                {
                    vehicle = Cache.PlayerPed.GetVehicleInFront();
                }

                if (vehicle == null) return;

                if (vehicle.Driver == Cache.PlayerPed)
                    Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);

                await vehicle.FadeOut(true);

                vehicle.IsPositionFrozen = true;
                vehicle.IsCollisionEnabled = false;

                EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);

                Entity attached = vehicle.GetEntityAttachedTo();

                if (attached is not null)
                {
                    attached.IsCollisionEnabled = false;
                    EventSystem.GetModule().Send("delete:entity", attached.NetworkId);

                    if (attached.Exists())
                        attached.Delete();
                }

                if (vehicle.Exists())
                    vehicle.Delete();
            }
        }

        [CommandInfo(new[] { "car", "veh" })]
        public class VehicleSpawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {

                Vehicle vehicle = null;
                
                if (Cache.PersonalVehicle is not null)
                    vehicle = Cache.PersonalVehicle.Vehicle;

                if (vehicle is not null)
                {
                    if (vehicle.Exists()) // personal vehicle
                    {
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

                int networkId = await EventSystem.Request<int>("vehicle:spawn", arguments.ElementAt(0));

                int vehId = API.NetworkGetEntityFromNetworkId(networkId);

                if (!API.DoesEntityExist(vehId)) return;

                vehicle = new Vehicle(vehId);

                Cache.PersonalVehicle = new State.VehicleState(vehicle);
                Cache.PlayerPed.Task.WarpIntoVehicle(Cache.PersonalVehicle.Vehicle, VehicleSeat.Driver);

                Cache.Player.User.SendEvent("vehicle:log:player", vehicle.NetworkId);

                await vehicle.FadeIn();
            }
        }

        [CommandInfo(new[] { "trailer" })]
        public class TrailerSpawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {

                Vehicle vehicle = null;

                if (Cache.PersonalTrailer is not null)
                    vehicle = Cache.PersonalTrailer.Vehicle;

                if (vehicle is not null)
                {
                    if (vehicle.Exists()) // personal vehicle
                    {
                        await vehicle.FadeOut(true);

                        vehicle.IsPositionFrozen = true;
                        vehicle.IsCollisionEnabled = false;

                        EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);

                        if (vehicle.Exists())
                            vehicle.Delete();

                        await BaseScript.Delay(500);
                    }
                }

                Vector3 charPos = Cache.PlayerPed.Position;
                Vector3 spawnPos = Vector3.Zero;
                float spawnHeading = 0f;

                Vector3 spawnRoad = Vector3.Zero;

                API.GetClosestVehicleNodeWithHeading(charPos.X, charPos.Y, charPos.Z, ref spawnPos, ref spawnHeading, 1, 3f, 0);
                API.GetRoadSidePointWithHeading(spawnPos.X, spawnPos.Y, spawnPos.Z, spawnHeading, ref spawnRoad);

                int networkId = await EventSystem.Request<int>("vehicle:trailer:spawn:position", arguments.ElementAt(0), spawnRoad.X, spawnRoad.Y, spawnRoad.Z, spawnHeading);

                int vehId = API.NetworkGetEntityFromNetworkId(networkId);

                if (!API.DoesEntityExist(vehId)) return;

                vehicle = new Vehicle(vehId);

                Cache.PersonalTrailer = new State.VehicleState(vehicle);
                Cache.Player.User.SendEvent("vehicle:log:player:trailer", vehicle.NetworkId);

                await vehicle.FadeIn();
            }
        }

        [CommandInfo(new[] { "c" })]
        public class VehicleSpawnerSafe : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {

                Vehicle vehicle = null;

                if (Cache.PersonalVehicle is not null)
                    vehicle = Cache.PersonalVehicle.Vehicle;

                if (vehicle is not null)
                {
                    if (vehicle.Exists()) // personal vehicle
                    {
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

                Vector3 charPos = Cache.PlayerPed.Position;
                Vector3 spawnPos = Vector3.Zero;
                float spawnHeading = 0f;

                Vector3 spawnRoad = Vector3.Zero;

                API.GetClosestVehicleNodeWithHeading(charPos.X, charPos.Y, charPos.Z, ref spawnPos, ref spawnHeading, 1, 3f, 0);
                API.GetRoadSidePointWithHeading(spawnPos.X, spawnPos.Y, spawnPos.Z, spawnHeading, ref spawnRoad);

                int networkId = await EventSystem.Request<int>("vehicle:spawn:position", arguments.ElementAt(0), spawnRoad.X, spawnRoad.Y, spawnRoad.Z, spawnHeading);

                int vehId = API.NetworkGetEntityFromNetworkId(networkId);

                if (!API.DoesEntityExist(vehId)) return;

                vehicle = new Vehicle(vehId);

                await vehicle.FadeIn();

                Cache.PersonalVehicle = new State.VehicleState(vehicle);

                Cache.Player.User.SendEvent("vehicle:log:player", vehicle.NetworkId);
            }
        }
        #endregion
    }
}
