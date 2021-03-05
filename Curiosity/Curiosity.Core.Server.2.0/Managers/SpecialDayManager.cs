using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class SpecialDayManager : Manager<SpecialDayManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("event:special", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "event:special");
                    return null;
                }
            }));
        }
    }
}
