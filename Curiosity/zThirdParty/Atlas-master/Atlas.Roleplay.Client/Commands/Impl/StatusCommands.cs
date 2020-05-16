using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Interface;
using System.Collections.Generic;
using System.Drawing;

namespace Atlas.Roleplay.Client.Commands.Impl
{
    public class StatusCommands
    {
        public class Online : CommandContext
        {
            [CommandInfo(new string[0])]
            public class NestedOnline : ICommand
            {
                public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
                {
                    Chat.SendLocalMessage("Status",
                        $"Det är just nu {AtlasPlugin.OnlinePlayers}/{AtlasPlugin.MaximumPlayers} spelare anslutna.",
                        Color.FromArgb(150, 150, 255));
                }
            }

            public override string[] Aliases { get; set; } = { "online", "players" };
            public override string Title { get; set; } = "Status";
            public override Color Color { get; set; } = Color.FromArgb(150, 150, 255);
            public override bool IsRestricted { get; set; }
        }
    }
}