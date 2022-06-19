using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
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
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.ADMINISTRATOR, Role.COMMUNITY_MANAGER, Role.DEVELOPER, Role.HEAD_ADMIN, Role.HELPER, Role.MODERATOR, Role.PROJECT_MANAGER, Role.SENIOR_ADMIN };


        static ConfigurationManager config => ConfigurationManager.GetModule();



        #region Entities

        [CommandInfo(new[] { "creator", "c" })]
        public class EntityCreator : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Entity ent = Cache.PlayerPed.GetEntityInFront();

                if (ent == null)
                {
                    Notify.Alert($"No entity found.");
                    return;
                }

                int popType = API.GetEntityPopulationType(ent.Handle);

                string name = ent.State.Get(StateBagKey.CURIOSITY_CREATED) ?? "unknown";

                Notify.Alert($"Entity Created By;~n~{name}~n~PopType: {(PopulationType)popType}");
            }
        }

        [CommandInfo(new[] { "del" })]
        public class EntityDespawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {

                if (arguments.Count == 0)
                {
                    Entity entityToDelete = Cache.PlayerPed.GetEntityInFront();
                    DeleteEntity(entityToDelete);
                }

                if (arguments.Count > 0)
                {
                    if (arguments[0] == "ped")
                    {
                        Ped[] peds = World.GetAllPeds();
                        int deleted = 0;
                        foreach (Ped ped in peds)
                        {
                            if (ped.Exists())
                            {
                                if (ped.IsDead && !ped.IsPlayer)
                                {
                                    DeleteEntity(ped);
                                    deleted++;
                                }
                            }
                        }
                        NotificationManager.GetModule().Info($"Processed all dead peds in area.<br />Count: {peds.Length} / Deleted: {deleted}");
                    }

                    if (arguments[0] == "prop")
                    {
                        bool force = arguments.Count > 1;
                        Prop[] props = World.GetAllProps();
                        int deleted = 0;
                        foreach (Prop prop in props)
                        {
                            if (prop.Exists())
                            {
                                int model = prop.Model.Hash;

                                if (config.PropsToDelete().Contains(model) && !force)
                                {
                                    DeleteEntity(prop);
                                    deleted++;
                                }
                                else if (force)
                                {
                                    DeleteEntity(prop);
                                    deleted++;
                                }
                            }
                        }
                        NotificationManager.GetModule().Info($"Processed all props in area.<br />Count: {props.Length} / Deleted: {deleted}");
                    }
                }
            }

            private async void DeleteEntity(Entity entityToDelete)
            {
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

                if (Cache.StaffVehicle is not null)
                    vehicle = Cache.StaffVehicle.Vehicle;

                if (vehicle is not null)
                {
                    if (vehicle.Exists()) // personal vehicle
                    {
                        if (vehicle.Driver == Cache.PlayerPed)
                            Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);

                        vehicle.IsPositionFrozen = true;
                        vehicle.IsCollisionEnabled = false;

                        await vehicle.FadeOut(true);

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

                Model vehModel = new Model(arguments.ElementAt(0));

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

                bool requestLogged = await EventSystem.GetModule().Request<bool>("onesync:request");

                if (!requestLogged) return;

                await BaseScript.Delay(1000);

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

                await BaseScript.Delay(100);

                if (vehModel?.IsLoaded ?? false)
                    vehModel.MarkAsNoLongerNeeded();

                API.NetworkRequestControlOfEntity(vehicle.Handle);
                API.SetNetworkIdExistsOnAllMachines(vehicle.NetworkId, true);
                API.SetNetworkIdCanMigrate(vehicle.NetworkId, true);
                API.SetVehicleHasBeenOwnedByPlayer(vehicle.Handle, true);

                bool b = await EventSystem.GetModule().Request<bool>("garage:set:vehicle", vehicle.NetworkId, (int)SpawnType.Vehicle);

                await BaseScript.Delay(100);

                if (b)
                {
                    vehicle.Position = Game.PlayerPed.Position;
                    vehicle.CreateBlip();
                    Cache.StaffVehicle = new State.VehicleState(vehicle);
                    Cache.Player.User.SendEvent("vehicle:log:staff", vehicle.NetworkId);

                    vehicle.IsPositionFrozen = false;
                    vehicle.IsCollisionEnabled = true;

                    if (vehicle.ClassType == VehicleClass.Helicopters && GetEntityHeightAboveGround(Cache.PlayerPed.Handle) > 10.0f)
                    {
                        SetHeliBladesFullSpeed(vehicle.Handle);
                    }
                    else
                    {
                        vehicle.PlaceOnGround();
                    }

                    vehicle.FadeIn();
                    vehicle.Opacity = 255;
                    vehicle.ResetOpacity();
                    API.SetVehicleExclusiveDriver_2(vehicle.Handle, Game.PlayerPed.Handle, 1);

                    Cache.PlayerPed.SetIntoVehicle(Cache.StaffVehicle.Vehicle, VehicleSeat.Driver);
                    return;
                }

                EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);
                vehicle.Delete();
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

                Model vehModel = new Model(arguments.ElementAt(0));

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

                vehicle = await World.CreateVehicle(vehModel, spawnRoad, spawnHeading);
                vehicle.Opacity = 0;

                if (vehModel?.IsLoaded ?? false)
                    vehModel.MarkAsNoLongerNeeded();

                API.SetNetworkIdExistsOnAllMachines(vehicle.NetworkId, true);
                API.SetNetworkIdCanMigrate(vehicle.NetworkId, true);
                API.SetVehicleHasBeenOwnedByPlayer(vehicle.Handle, true);

                bool b = await EventSystem.GetModule().Request<bool>("garage:set:vehicle", vehicle.NetworkId, (int)SpawnType.Vehicle);

                if (b)
                {
                    Cache.PersonalTrailer = new State.VehicleState(vehicle);
                    Cache.Player.User.SendEvent("vehicle:log:player:trailer", vehicle.NetworkId);
                    vehicle.FadeIn();
                    return;
                }
                EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);
                vehicle.Delete();
            }

        }
        #endregion
    }
}
