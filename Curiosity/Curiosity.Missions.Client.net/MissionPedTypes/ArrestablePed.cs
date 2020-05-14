using CitizenFX.Core;
using Curiosity.Global.Shared.net.NPCType;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.MissionPeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.MissionPedTypes
{
    internal class ArrestablePed : WorldPed
    {
        private readonly Ped Ped;

        public ArrestablePed(int handle) : base(handle)
        {
            Ped = this;

            if (Decorators.GetBoolean(Ped.Handle, Decorators.DECOR_PED_INFLUENCE_ALCAHOL)
                || Decorators.GetBoolean(Ped.Handle, Client.DECOR_NPC_DRUG_ALCOHOL))
            {
                Profile = new NpcArrestable(true, (int)Ped.Gender);
            }
            else
            {
                bool influence = (Client.Random.Next(30) >= 28);
                Profile = new NpcArrestable(influence, (int)Ped.Gender);
            }
        }
    }
}
