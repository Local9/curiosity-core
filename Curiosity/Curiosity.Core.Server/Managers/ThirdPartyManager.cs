using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class ThirdPartyManager : Manager<ThirdPartyManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry.Add("txAdmin:events:scheduledRestart", new Action<dynamic>(async data =>
            {
                Util.Notify.Send(notification: eNotification.NOTIFICATION_WARNING, message: $"Server restart: {data.secondsRemaining / 60} minute(s)", position: "top-middle", playSound: true);

                if (data.secondsRemaining == 60)
                {
                    await BaseScript.Delay(45000);
                    
                    Logger.Warn($"15 seconds before restart, saving all players.");

                    foreach (var kvp in PluginManager.ActiveUsers)
                    {
                        await kvp.Value.Character.Save();
                    }
                }
            }));
        }
    }
}
