using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes.Environment
{
    class Trains
    {
        static Server server = Server.GetInstance();

        static bool IsTrainActivated;

        static public void Init()
        {
            server.RegisterEventHandler("environment:train:activate", new Action(OnActivateTrains));
        }

        static async void OnActivateTrains()
        {
            if (!IsTrainActivated && Server.players.Count() == 1) // new session
            {
                Server.TriggerClientEvent("environment:client:train:activate", API.GetHostId());
            }
            else
            {
                if (Server.players.Count() == 0)
                    IsTrainActivated = false;

                long gameTimer = API.GetGameTimer();
                while ((API.GetGameTimer() - gameTimer) < 15000)
                {
                    await BaseScript.Delay(0);
                }
                OnActivateTrains();
            }
        }
    }
}
