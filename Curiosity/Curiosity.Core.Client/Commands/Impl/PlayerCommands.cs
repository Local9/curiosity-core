using Curiosity.Core.Client.Environment;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Curiosity.Core.Client.Commands.Impl
{
    public class PlayerCommands : CommandContext
    {
        [CommandInfo(new[] { "unstuck", })]
        public class InviteToParty : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Position position = new Position(-542.1675f, -216.1688f, -216.1688f, 276.3713f);
                await SafeTeleport.TeleportFade(player.Entity.Id, position);
            }
        }

        public override string[] Aliases { get; set; } = { "player", "p", "me" };
        public override string Title { get; set; } = "Player Commands";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; }
        public override List<Role> RequiredRoles { get; set; }
    }
}
