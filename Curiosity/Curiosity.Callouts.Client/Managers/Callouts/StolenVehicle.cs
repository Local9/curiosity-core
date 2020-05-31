using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Utils;
using System.Collections.Generic;
using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class StolenVehicle : Callout
    {
        private Ped criminal;
        private Vehicle stolenVehicle;

        public List<VehicleHash> vehicleHashes = new List<VehicleHash>()
        {
            VehicleHash.Oracle2,
            VehicleHash.Panto,
            VehicleHash.Sandking,
            VehicleHash.SlamVan
        };

        public List<PedHash> pedHashes = new List<PedHash>()
        {
            PedHash.Tourist01AFM,
            PedHash.Tourist01AFY,
            PedHash.Lost01GFY,
            PedHash.Lost01GMY,
            PedHash.MexGang01GMY,
            PedHash.Families01GFY,
            PedHash.BallaOrig01GMY,
            PedHash.Korean01GMY
        };

        public StolenVehicle(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        internal override async void Prepare()
        {
            base.Prepare();

            stolenVehicle = await Vehicle.Spawn(vehicleHashes.Random(),
                Players[0].Character.Position.AroundStreet(250f, 500f));
            RegisterVehicle(stolenVehicle);
            Logger.Log(stolenVehicle.Name);

            criminal = await Ped.Spawn(pedHashes.Random(), stolenVehicle.Position);
            criminal.Fx.RelationshipGroup = (uint)Collections.RelationshipHash.Prisoner;
            RegisterPed(criminal);

            criminal.PutInVehicle(stolenVehicle);
            criminal.Task.CruiseWithVehicle(stolenVehicle.Fx, float.MaxValue,
                (int)Collections.CombinedVehicleDrivingFlags.Fleeing);
            criminal.AttachBlip(BlipColor.Red, true);

            PursuitManager.StartNewPursuit(this);

            base.IsSetup = true;
        }

        internal override void Tick()
        {
            if (criminal == null)
            {
                Logger.Log("Criminal was null");
                End(true);
                return;
            }

            switch (progress)
            {
                case 1:
                    if (!criminal.IsDead) return;
                    progress++;
                    break;

                default:
                    End();
                    break;
            }
        }
    }
}
