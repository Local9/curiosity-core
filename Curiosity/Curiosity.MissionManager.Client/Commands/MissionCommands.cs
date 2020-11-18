﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Attributes;
using System;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Commands
{
    class MissionCommands : BaseScript
    {
        public MissionCommands()
        {
            API.RegisterCommand("missionTest", new Action<int, List<object>, string>(CommandMissionTest), false);
        }

        private void CommandMissionTest(int playerHandle, List<object> args, string raw)
        {
            if (!Cache.Player.User.IsDeveloper) return;

            if (args.Count == 0) Screen.ShowNotification("Mission Argument~n~/missionTest <MissionId>");

            if (Mission.isOnMission)
                Mission.currentMission.End();


            string missionId = $"{args[0]}";

            Mission.missions.ForEach(mission =>
            {
                MissionInfo missionInfo = Functions.GetMissionInfo(mission);

                if (missionInfo == null)
                {
                    Screen.ShowNotification("Mission Info Attribute not found.");
                    return;
                }

                if (missionInfo.id == missionId)
                    Functions.StartMission(mission);
            });
        }
    }
}
