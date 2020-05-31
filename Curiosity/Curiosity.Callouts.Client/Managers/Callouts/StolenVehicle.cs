using CitizenFX.Core;
using Curiosity.Callouts.Client.Utils;

using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class StolenVehicle : Callout
    {
        private Ped criminal;
        private Vehicle stolenVehicle;

        public StolenVehicle(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        internal override async void Prepare()
        {
            base.Prepare();

            stolenVehicle = await Vehicle.Spawn(VehicleHash.Oracle2,
                Players[0].Character.Position.AroundStreet(250f, 1000f));
            RegisterVehicle(stolenVehicle);
            Logger.Log(stolenVehicle.Name);

            criminal = await Ped.Spawn(PedHash.Tourist01AFM, stolenVehicle.Position);
            criminal.Fx.RelationshipGroup = (uint)Collections.RelationshipHash.Prisoner;
            RegisterPed(criminal);

            criminal.PutInVehicle(stolenVehicle);
            criminal.Task.CruiseWithVehicle(stolenVehicle.Fx, float.MaxValue,
                (int)Collections.CombinedVehicleDrivingFlags.Fleeing);
            criminal.AttachBlip(BlipColor.Red, true);

            PursuitManager.StartNewPursuit(this);
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
