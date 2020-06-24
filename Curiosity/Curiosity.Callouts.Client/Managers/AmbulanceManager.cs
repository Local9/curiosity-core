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
    internal class AmbulanceManager : BaseScript
    {
        public static bool IsActive = false;
        static Ped PedToHelp;
        static int managerProgress = 0;

        Vehicle AmbulanceVehicle;

        Ped AmbulanceDriver;
        Ped AmbulancePassenger;
        
        Vector3 PickupLocation;

        [Tick]
        async Task OnAmbulanceTick()
        {
            if (PedToHelp == null) return;

#if DEBUG
            if (PlayerManager.IsDeveloper && AmbulanceVehicle != null && PlayerManager.IsDeveloperUIActive)
                Screen.ShowSubtitle($"Dis: {Game.PlayerPed.Position.Distance(AmbulanceVehicle.Position)} | prog: {managerProgress}");
#endif

            Sequence(managerProgress);
            await BaseScript.Delay(500);
        }

        async void Sequence(int progress)
        {
            /// C:\Users\lacol\Documents\dumps\Dump20200623

            switch (progress)
            {
                case 0:
                    Logger.Log($"Clear if exists");
                    AmbulanceDriver?.Dismiss();
                    AmbulancePassenger?.Dismiss();
                    AmbulanceVehicle?.Dismiss();
                    managerProgress = 1;
                    break;
            }
        }
    }
}
