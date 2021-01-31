using CitizenFX.Core;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Commands.Impl
{
    public class StaffCommands : CommandContext
    {
        public override string[] Aliases { get; set; } = { "staff", "s" };
        public override string Title { get; set; } = "Staff Commands";
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = STAFF_ROLES;

        #region Player Based Commands
        [CommandInfo(new[] { "revive" })]
        public class WorldWeather : ICommand
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
    }
}
