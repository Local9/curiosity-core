using CitizenFX.Core;
using Curiosity.Callouts.Server.Utils;
using Curiosity.Callouts.Shared.EventWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Server.Managers
{
    class CalloutDistributorManager : BaseScript
    {
        async void GetAvailablePlayers()
        {
            var identifier = Shared.Utils.Utility.GenerateIdentifier();
            Events.Client.Callout.RequestFreeCops.Trigger(identifier);

            EventHandlers[Events.Server.Callout.FreeCopsResponse] += new Action<Player, int>(
                (player, callbackIdentifier) =>
                {
                    if (callbackIdentifier != identifier) return;
                });
        }
    }
}
