using CitizenFX.Core;
using Curiosity.Missions.Client.DataClasses;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.MissionPedTypes;

namespace Curiosity.Missions.Client.Scripts.PedCreators
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
