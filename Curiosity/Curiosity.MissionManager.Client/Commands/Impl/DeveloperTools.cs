using CitizenFX.Core;
using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System.Collections.Generic;
using System.Drawing;

namespace Curiosity.MissionManager.Client.Commands.Impl
{
    public class DeveloperTools : CommandContext
    {
        public override string[] Aliases { get; set; } = { "dev", "d" };
        public override string Title { get; set; } = "Developer Commands";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER, Role.PROJECT_MANAGER };

        #region Player
        //[CommandInfo(new[] { "god" })]
        //public class Godmode : ICommand
        //{
        //    public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
        //    {
        //        player.Entity.ToggleGodMode();
        //        if (Game.PlayerPed.IsInvincible)
        //        {
        //            Chat.SendLocalMessage("God Mode: Enabled");
        //        }
        //        else
        //        {
        //            Chat.SendLocalMessage("God Mode: Disabled");
        //        }
        //    }
        //}
        #endregion

        #region Mission Helper
        [CommandInfo(new[] { "duty" })]
        public class Godmode : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                bool dutyActive = false;
                string job = "police";

                if (arguments.Count >= 1)
                {
                    job = arguments[0];
                }

                if (arguments.Count == 2)
                {
                    dutyActive = arguments[1] == "1";
                }

                // EventSystem.GetModule().Request<object>(LegacyEvents.Client.PoliceDutyEvent, true, dutyActive, job);
                BaseScript.TriggerEvent(LegacyEvents.Client.PoliceDutyEvent, true, dutyActive, job); // for legacy resources
            }
        }
        #endregion
    }
}
