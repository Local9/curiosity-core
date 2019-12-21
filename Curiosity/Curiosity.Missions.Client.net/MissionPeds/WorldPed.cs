using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using CitizenFX.Core.NaturalMotion;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    class WorldPed
    {
        // RAGE ENGINE
        Ped _ped;

        // CURIOSITY
        string _firstname;
        string _surname;

        int _health;

        protected WorldPed()
        {
            this._ped.Health = 200;
            
            // 1/10 chance of armor
            if (Client.Random.Next(10) == 9)
                this._ped.Armor = Client.Random.Next(100);
        }
    }
}
