using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.MissionPedTypes;

namespace Curiosity.Missions.Client.net.Scripts.PedCreators
{
    static class InteractivePedCreator
    {
        public static InteractivePed Ped(Ped ped, Alertness alertness = Alertness.Nuetral)
        {
            InteractivePed interactivePed;

            ped.CanPlayGestures = true;
            ped.SetCanPlayAmbientAnims(true);
            ped.SetCanEvasiveDive(true);
            ped.SetPathCanUseLadders(true);
            ped.SetPathCanClimb(true);
            ped.AlwaysDiesOnLowHealth = Client.Random.Next(2) == 1;
            ped.SetAlertness(alertness);
            ped.SetCombatAttributes(CombatAttributes.AlwaysFight, true);
            Function.Call((Hash)8116279360099375049L, new InputArgument[] { ped.Handle, 0, 0 });
            ped.SetConfigFlag(281, true);

            API.SetEntityAsMissionEntity(ped.Handle, false, false);

            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;

            ped.NeverLeavesGroup = false;

            interactivePed = new InteractablePed(ped.Handle);

            return interactivePed;
        }
    }
}
