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

                EventSystem.GetModule().Send("entity:delete", vehicle.NetworkId);

                if (vehicle.Exists())
                    vehicle.Delete();
            }
        }

        [CommandInfo(new[] { "car", "veh" })]
        public class VehicleSpawner : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = Cache.PersonalVehicle;

                if (vehicle != null)
                {
                    if (vehicle.Exists()) // personal vehicle
                    {
                        if (vehicle.Driver == Cache.PlayerPed)
                            Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);

                        await vehicle.FadeOut(true);

                        vehicle.IsPositionFrozen = true;
                        vehicle.IsCollisionEnabled = false;

                        EventSystem.GetModule().Send("entity:delete", vehicle.NetworkId);

                        if (vehicle.Exists())
                            vehicle.Delete();

                        await BaseScript.Delay(500);
                    }
                }

                if (Cache.Entity != null && Cache.Entity.Vehicle != null)
                {
                    if (Cache.Entity.Vehicle.Exists()) // get vehicle player is in
                    {
                        vehicle = Cache.Entity.Vehicle;

                        if (vehicle.Driver == Cache.PlayerPed)
                            Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);

                        await vehicle.FadeOut(true);

                        vehicle.IsPositionFrozen = true;
                        vehicle.IsCollisionEnabled = false;

                        EventSystem.GetModule().Send("entity:delete", vehicle.NetworkId);

                        if (vehicle.Exists())
                            vehicle.Delete();

                        await BaseScript.Delay(500);
                    }
                }

                int networkId = await EventSystem.Request<int>("vehicle:spawn", arguments.ElementAt(0));

                int vehId = API.NetworkGetEntityFromNetworkId(networkId);

                if (!API.DoesEntityExist(vehId)) return;

                vehicle = new Vehicle(vehId);

                await vehicle.FadeIn();

                Cache.PersonalVehicle = vehicle;
                Cache.PlayerPed.Task.WarpIntoVehicle(Cache.PersonalVehicle, VehicleSeat.Driver);

                Cache.Player.User.SendEvent("vehicle:log:player", vehicle.NetworkId);
            }
        }
        #endregion
    }
}
