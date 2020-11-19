using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

                if (arguments.Count > 0)
                {
                    dutyActive = arguments[0] == "1";
                }

                BaseScript.TriggerEvent("curiosity:Client:Interface:Duty", true, dutyActive, "police");
            }
        }
        #endregion
    }
}
