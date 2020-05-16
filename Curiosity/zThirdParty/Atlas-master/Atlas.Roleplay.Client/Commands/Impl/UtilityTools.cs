using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Events;
using Atlas.Roleplay.Client.Interface;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Atlas.Roleplay.Client.Commands.Impl
{
    public class UtilityTools
    {
        public class Reload : CommandContext
        {
            [CommandInfo(new string[0])]
            public class NestedReload : ICommand
            {
                public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
                {
                    API.ClearPedTasksImmediately(entity.Id);
                    API.ClearPedSecondaryTask(entity.Id);
                    API.SetNuiFocus(false, false);

                    World.GetAllProps().Where(self => self.IsAttachedTo(Entity.FromHandle(entity.Id))).ToList()
                        .ForEach(self => self.Detach());

                    Chat.SendGlobalMessage("Reload", "Laddade om karaktären!", Color.FromArgb(255, 0, 0));
                }
            }

            public override string[] Aliases { get; set; } = { "reload", "fix" };
            public override string Title { get; set; } = "Reload";
            public override Color Color { get; set; } = Color.FromArgb(255, 0, 0);
            public override bool IsRestricted { get; set; } = false;
        }

        public class Report : CommandContext
        {
            [CommandInfo(new string[0])]
            public class NestedReport : ICommand
            {
                public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
                {
                    EventSystem.GetModule().Send("report:create", player.Name, player.Character.Fullname,
                        string.Join(" ", arguments));
                }
            }

            public override string[] Aliases { get; set; } = { "rep", "report", "bug" };
            public override string Title { get; set; } = "Rapporter";
            public override Color Color { get; set; } = Color.FromArgb(255, 0, 0);
            public override bool IsRestricted { get; set; } = false;
        }
    }
}