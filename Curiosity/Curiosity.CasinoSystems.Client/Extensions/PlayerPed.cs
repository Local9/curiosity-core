using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client.Extensions
{
    static class PlayerPed
    {
        public static bool IsFemale(this Ped ped)
        {
            return ped.Gender == Gender.Female;
        }
    }
}
