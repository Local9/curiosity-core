﻿using CitizenFX.Core;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.MissionPedTypes;
using Curiosity.Missions.Client.net.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.Scripts.PedCreators
{
    static class WorldPedCreator
    {
        static public WorldPed Create(Ped ped, PedType missionPedTypes)
        {
            WorldPed worldPed;
            switch (missionPedTypes)
            {
                case PedType.ARRESTABLE:
                    worldPed = new ArrestablePed(ped.Handle);
                    break;
                default:
                    worldPed = new ArrestablePed(ped.Handle); // make a default
                    break;
            }

            return worldPed;
        }
    }
}
