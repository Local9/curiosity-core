using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.MissionPedTypes;

namespace Curiosity.Missions.Client.net.Scripts.PedCreators
{
    class NormalPedCreator
    {
        public static NormalPed Setup(Ped ped, bool canArrest = false, Alertness alertness = Alertness.Nuetral)
        {
            NormalPed normalPed;

            ped.CanPlayGestures = true;
            ped.SetCanPlayAmbientAnims(true);
            ped.SetCanEvasiveDive(true);
            ped.SetPathCanUseLadders(true);
            ped.SetPathCanClimb(true);
            ped.AlwaysDiesOnLowHealth = Client.Random.Next(2) == 1;
            ped.SetAlertness(alertness);
            Function.Call((Hash)8116279360099375049L, new InputArgument[] { ped.Handle, 0, 0 });
            ped.SetConfigFlag(281, true);

            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;

            normalPed = new PedNormal(ped.Handle, canArrest);

            return normalPed;
        }
    }
}

