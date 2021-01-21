﻿using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class JobManager : Manager<JobManager>
    {
        private const string JOB_POLICE_DUTY = "job:police:duty";
        private const string JOB_POLICE_ARREST = "job:police:arrest";
        private ConfigManager config;

        private bool pending = false;

        public override void Begin()
        {
            config = ConfigManager.ConfigInstance;

            EventSystem.GetModule().Attach(JOB_POLICE_DUTY, new AsyncEventCallback(async metadata =>
            {
                if (pending) return null;

                pending = true;

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                    return null;

                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];

                Vector3 position = Vector3.Zero;
                position.X = metadata.Find<float>(0);
                position.Y = metadata.Find<float>(1);
                position.Z = metadata.Find<float>(2);

                bool result = config.IsNearLocation(position, JOB_POLICE_DUTY, 5.0f);

                if (result)
                {
                    user.CurrentJob = "Police Officer";
                }

                Instance.ExportDictionary["curiosity-missions"].JobEvent(metadata.Sender, JOB_POLICE_DUTY);

                await BaseScript.Delay(5000);

                pending = false;

                return null;
            }));
            
            EventSystem.GetModule().Attach(JOB_POLICE_ARREST, new AsyncEventCallback(async metadata =>
            {
                if (pending) return null;
                
                pending = true;

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                    return null;

                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];

                Vector3 position = Vector3.Zero;
                position.X = metadata.Find<float>(0);
                position.Y = metadata.Find<float>(1);
                position.Z = metadata.Find<float>(2);

                bool result = config.IsNearLocation(position, JOB_POLICE_ARREST, 5.0f);

                Instance.ExportDictionary["curiosity-missions"].JobEvent(metadata.Sender, JOB_POLICE_ARREST);

                await BaseScript.Delay(5000);

                pending = false;

                return null;
            }));

            Instance.ExportDictionary.Add("SetUserJob", new Func<string, string, bool>(
                (playerHandle, job) => {

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        return false;

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        return false;

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];
                    user.CurrentJob = job;

                    return true;
                }));
        }
    }
}
