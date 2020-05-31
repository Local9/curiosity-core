using Curiosity.Callouts.Client.Managers;
using Curiosity.Callouts.Client.Utils;
using System;
using System.Collections.Generic;

namespace Curiosity.Callouts.Client.Classes
{
    internal class Pursuit
    {
        public Callout Callout { get; }
        public readonly List<Tuple<Ped, Vehicle>> cops = new List<Tuple<Ped, Vehicle>>();

        internal Pursuit(Callout callout)
        {
            Callout = callout;
        }

        public void End()
        {
            cops.ForEach(tuple =>
            {
                if (tuple.Item2 != null)
                    tuple.Item1?.Task.CruiseWithVehicle(tuple.Item2.Fx, 30f,
                        (int)Collections.CombinedVehicleDrivingFlags.Normal);
            });
        }
    }
}
