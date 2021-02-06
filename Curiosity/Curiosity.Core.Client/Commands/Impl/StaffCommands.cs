using CitizenFX.Core;
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
                    Game.PlayerPed.Weapons.Give(w, 999, false, true);
                    Game.PlayerPed.Weapons[w].InfiniteAmmo = true;
                    Game.PlayerPed.Weapons[w].InfiniteAmmoClip = true;
                });

                Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed);
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
                    vehicle = Game.PlayerPed.GetVehicleInFront();
                }

                if (vehicle == null) return;

                EventSystem.GetModule().Send("entity:delete", vehicle.NetworkId);

                await BaseScript.Delay(2000);

                while (vehicle.Exists())
                {
                    await BaseScript.Delay(100);
                    await vehicle.FadeOut();

                    vehicle.Delete();
                }

                vehicle?.Delete();
            }
        }
        #endregion
    }
}
