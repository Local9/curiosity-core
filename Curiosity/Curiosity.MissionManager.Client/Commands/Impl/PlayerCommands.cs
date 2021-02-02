using Curiosity.MissionManager.Client.Events;
using Curiosity.Systems.Library.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace Curiosity.MissionManager.Client.Commands.Impl
{
    public class PlayerCommands : CommandContext
    {
        static EventSystem EventSystem => EventSystem.GetModule();
        public override string[] Aliases { get; set; } = { "dispatch", "pd" };
        public override string Title { get; set; } = "Dispatch Commands";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; } = false;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { };


    }
}
