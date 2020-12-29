﻿using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Events;

namespace Curiosity.MissionManager.Client.Managers
{
    public class NotificationManager : Manager<NotificationManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("system:notification:basic", new EventCallback(metadata =>
            {
                Notify.Custom(metadata.Find<string>(0));
                return null;
            }));
        }
    }
}