using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Curiosity.Systems.Server.Commands.Impl
{
    public class DeveloperTools : CommandContext
    {
        public override string[] Aliases { get; set; } = { "srv" };
        public override string Title { get; set; } = "Server Commands";
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER, Role.PROJECT_MANAGER };

        #region Vehicles
        [CommandInfo(new[] { "vehicle", "veh", "car" })]
        public class VehicleSpawner : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                try
                {
                    if (arguments.Count <= 0) return;
                    var model = API.GetHashKey(arguments.ElementAt(0));

                    Vector3 pos = player.Character.Position;
                    int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);
                }
                catch (Exception)
                {
                    // Ignored
                }
            }
        }

        #endregion
    }
}
