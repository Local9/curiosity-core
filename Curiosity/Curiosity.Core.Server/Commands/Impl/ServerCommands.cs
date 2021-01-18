using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Curiosity.Core.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Commands.Impl
{
    public class ServerCommands : CommandContext
    {
        public override string[] Aliases { get; set; } = { "srv" };
        public override string Title { get; set; } = "Server Commands";
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER, Role.PROJECT_MANAGER };

        #region Vehicles ONESYNC
        //[CommandInfo(new[] { "vehicle", "veh", "car" })]
        //public class VehicleSpawner : ICommand
        //{
        //    public async void On(CuriosityUser user, Player player, List<string> arguments)
        //    {
        //        try
        //        {
        //            if (arguments.Count <= 0) return;
        //            var model = API.GetHashKey(arguments.ElementAt(0));
        //            float x = arguments.ElementAt(1).ToFloat();
        //            float y = arguments.ElementAt(2).ToFloat();
        //            float z = arguments.ElementAt(3).ToFloat();

        //            Vector3 pos = new Vector3(x, y, z);
        //            int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);
        //        }
        //        catch (Exception)
        //        {
        //            // Ignored
        //        }
        //    }
        //}
        #endregion
    }
}
