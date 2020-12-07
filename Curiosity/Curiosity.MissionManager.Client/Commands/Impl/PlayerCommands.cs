using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Curiosity.MissionManager.Client.Commands.Impl
{
    public class PlayerCommands : CommandContext
    {
        public override string[] Aliases { get; set; } = { "dispatch", "pd" };
        public override string Title { get; set; } = "Player Commands";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; } = false;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { };

        [CommandInfo(new[] { "coroner", "c" })]
        public class DeveloperDuty : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                List<Ped> peds = World.GetAllPeds().Where(x => x.IsDead && x.IsInRangeOf(Game.PlayerPed.Position, 15f)).ToList();

                peds.ForEach(async p =>
                {
                    p.MarkAsNoLongerNeeded();
                    await p.FadeOut();
                    p.Delete();

                    int pedHandle = p.Handle;

                    if (p.Exists())
                    {
                        API.RemovePedElegantly(ref pedHandle);
                        API.DeleteEntity(ref pedHandle);
                    }
                });
            }
        }

    }
}
