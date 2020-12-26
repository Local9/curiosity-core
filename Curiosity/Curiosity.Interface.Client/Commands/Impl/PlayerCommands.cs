using Curiosity.Interface.Client.Environment.Entities;
using Curiosity.Interface.Client.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Curiosity.Interface.Client.Commands.Impl
{
    public class PlayerCommands : CommandContext
    {
        [CommandInfo(new[] { "invite", "i" })]
        public class InviteToParty : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {

            }
        }

        [CommandInfo(new[] { "accept", "a" })]
        public class AcceptInvite : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                List<string> response = await EventSystem.GetModule().Request<List<string>>("party:get:members");
                response.ForEach((string member) =>
                {

                });
            }
        }

        public override string[] Aliases { get; set; } = { "party", "p" };
        public override string Title { get; set; } = "Party";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; }
        public override List<Role> RequiredRoles { get; set; }
    }
}
