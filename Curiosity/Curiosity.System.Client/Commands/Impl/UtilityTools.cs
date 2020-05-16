using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.System.Client.Environment.Entities;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Curiosity.System.Client.Commands.Impl
{
    public class UtilityTools
    {
        public class Reload : CommandContext
        {
            [CommandInfo(new string[0])]
            public class NestedReload : ICommand
            {
                public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
                {
                    API.ClearPedTasksImmediately(entity.Id);
                    API.ClearPedSecondaryTask(entity.Id);
                    API.SetNuiFocus(false, false);

                    World.GetAllProps().Where(self => self.IsAttachedTo(Entity.FromHandle(entity.Id))).ToList()
                        .ForEach(self => self.Detach());

                    // Chat.SendGlobalMessage("Reload", "Reloaded Character!", Color.FromArgb(255, 0, 0));
                }
            }

            public override string[] Aliases { get; set; } = { "reload", "fix" };
            public override string Title { get; set; } = "Reload";
            public override Color Color { get; set; } = Color.FromArgb(255, 0, 0);
            public override bool IsRestricted { get; set; } = false;
        }
    }
}