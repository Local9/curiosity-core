
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
            ped.SetAlertness(alertness);
            Function.Call((Hash)8116279360099375049L, new InputArgument[] { ped.Handle, 0, 0 });
            ped.SetConfigFlag(281, true);

            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;

            interactivePed = new InteractablePed(ped.Handle);

            return interactivePed;
        }

        public static InteractivePed Ped(Ped ped, RelationshipGroup relationshipGroup, Alertness alertness = Alertness.Nuetral)
        {
            InteractivePed interactivePed;

            ped.CanPlayGestures = true;
            ped.SetCanPlayAmbientAnims(true);
            ped.SetCanEvasiveDive(true);
            ped.SetPathCanUseLadders(true);
            ped.SetPathCanClimb(true);
            ped.SetAlertness(alertness);
            Function.Call((Hash)8116279360099375049L, new InputArgument[] { ped.Handle, 0, 0 });
            ped.SetConfigFlag(281, true);

            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;

            ped.RelationshipGroup = relationshipGroup;

            interactivePed = new InteractablePed(ped.Handle);

            return interactivePed;
        }
    }
}
