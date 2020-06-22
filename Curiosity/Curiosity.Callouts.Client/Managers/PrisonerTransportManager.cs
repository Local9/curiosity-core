using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Curiosity.Callouts.Client.Utils.Collections;
using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers
{
    internal class PrisonerTransportManager : BaseScript
    {
        public static bool IsActive = false;
        static List<Ped> Prisoners;
        static int managerProgress = 0;

        Vehicle TransportVehicle;
        Ped TransportDriver;
        Vector3 PickupLocation;

        [Tick]
        private async Task OnPrisonerTransportClick()
        {
            if (Prisoners.Count == 0) return;
        }
    }
}
