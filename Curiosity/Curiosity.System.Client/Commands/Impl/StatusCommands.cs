using System.Collections.Generic;
using System.Drawing;
using Curiosity.System.Client.Environment.Entities;
using Curiosity.System.Client.Interface;

namespace Curiosity.System.Client.Commands.Impl
{
    public class StatusCommands
    {
        public class Online : CommandContext
        {
            [CommandInfo(new string[0])]
            public class NestedOnline : ICommand
            {
                public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
                {
                    //Chat.SendLocalMessage("Status",
                    //    $"Det är just nu {CuriosityPlugin.OnlinePlayers}/{CuriosityPlugin.MaximumPlayers} spelare anslutna.",
                    //    Color.FromArgb(150, 150, 255));
                }
            }

            public override string[] Aliases { get; set; } = {"online", "players"};
            public override string Title { get; set; } = "Status";
            public override Color Color { get; set; } = Color.FromArgb(150, 150, 255);
            public override bool IsRestricted { get; set; }
        }
    }
}