using CitizenFX.Core.Native;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Utils;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class CasinoManager : Manager<CasinoManager>
    {
        string vehicleForToday = string.Empty;

        List<string> randomVehicle = new List<string>()
        {
            "thrax",
            "TURISMO2",
            "INFERNUS2",
            "jester3",
            "schlagen",
            "taipan",
            "gauntlet3",
            "stafford",
            "MAMBA",
            "swinger",
            "locust",
            "s80",
            "caracara2",
            "deveste",
            "neo",
            "stromberg",
            "krieger",
            "gauntlet4",
            "flashgt",
            "ARDENT",
            "drafter",
            "komoda",
            "everon",
            "emerus",
            "vstr",
            "comet4",
            "OPPRESSOR",
            "clique",
            "formula",
            "formula2"
        };

        public override void Begin()
        {
            vehicleForToday = randomVehicle[Utility.RANDOM.Next(randomVehicle.Count)];

            EventSystem.Attach("casino:vehicle", new EventCallback(metadata =>
            {
                return vehicleForToday;
            }));
        }
    }
}
