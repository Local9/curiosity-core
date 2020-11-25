using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Curiosity.MissionManager.Client.Commands.Impl
{
    public class DeveloperTools : CommandContext
    {
        static EventSystem EventSystem => EventSystem.GetModule();

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

        //[CommandInfo(new[] { "vehicle", "veh", "car" })]
        //public class VehicleSpawner : ICommand
        //{
        //    public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
        //    {
        //        try
        //        {
        //            if (arguments.Count <= 0) return;

        //            var model = new Model(API.GetHashKey(arguments.ElementAt(0)));

        //            if (!model.IsValid || !model.IsVehicle) return;

        //            if (Game.PlayerPed.IsInVehicle())
        //                Game.PlayerPed.CurrentVehicle.Delete();

        //            var position = entity.Position;
        //            var vehicle = await World.CreateVehicle(model, position.AsVector(), position.Heading);

        //            entity.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
        //        }
        //        catch (Exception)
        //        {
        //            // Ignored
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

        [CommandInfo(new [] { "mission" })]
        public class CreateMission : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                if (arguments.Count == 0) Notify.Error("Mission Argument~n~/mission <MissionId>");

                if (Mission.isOnMission)
                    Mission.currentMission.End();


                string missionId = $"{arguments[0]}";

                Mission.missions.ForEach(mission =>
                {
                    MissionInfo missionInfo = Functions.GetMissionInfo(mission);

                    if (missionInfo == null)
                    {
                        Notify.Error("Mission Info Attribute not found.");
                        return;
                    }

                    if (missionInfo.id == missionId)
                    {
                        EventSystem.Request<bool>("mission:activate", missionInfo.id, missionInfo.unique);

                        Functions.StartMission(mission);
                    }
                });
            }
        }
        #endregion
    }
}
