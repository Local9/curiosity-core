using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.GameWorld.Client.net.Classes.Environment
{
    class WorldLimits
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
        }

        static void OnPlayerSpawned(dynamic spawndata)
        {
            ExpandWorldLimits(-10000.0f, -12000.0f, -30.0f);
            ExpandWorldLimits(10000.0f, 12000.0f, 30.0f);
        }
    }
}
