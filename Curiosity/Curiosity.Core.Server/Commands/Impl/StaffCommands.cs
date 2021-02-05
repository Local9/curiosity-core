using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Commands.Impl
{
    public class StaffCommands : CommandContext
    {
        public override string[] Aliases { get; set; } = { "srvStaff", "sst" };
        public override string Title { get; set; } = "Staff Server Commands";
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.ADMINISTRATOR, Role.COMMUNITY_MANAGER, Role.DEVELOPER, Role.HEAD_ADMIN, Role.HELPER, Role.MODERATOR, Role.PROJECT_MANAGER, Role.SENIOR_ADMIN };

        #region Player Based Commands
        [CommandInfo(new[] { "revive" })]
        public class PlayerRevive : ICommand
        {
            public void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnChatMessage(player, $"Missing argument.");
                    return;
                }

                string arg = arguments.ElementAt(0);
                if (!int.TryParse(arg, out int handle))
                {
                    ChatManager.OnChatMessage(player, $"Argument is not a valid number.");
                    return;
                }

                if (!PluginManager.ActiveUsers.ContainsKey(handle))
                {
                    ChatManager.OnChatMessage(player, $"Player not found.");
                    return;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[handle];
                curiosityUser.Send("character:respawnNow");

                ChatManager.OnChatMessage(player, $"Player '{curiosityUser.LatestName}' respawned.");
            }
        }
        #endregion

        #region Vehicles ONESYNC
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

                    // int netId = API.NetworkGetNetworkIdFromEntity(vehicleId);

                    // Logger.Debug($"VehId: {vehicleId} NedId: {netId}");

                    if (vehicleId > 0)
                    {
                        PluginManager.ActiveUsers[player.Handle.ToInt()].PersonalVehicle = vehicleId;
                        Logger.Debug($"Veh: {vehicleId} assigned to {player.Handle}");
                    }
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
