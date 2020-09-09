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
    internal class ImpoundManager : BaseScript
    {
        public static bool IsActive = false;
        static Vehicle VehToImpound;
        static int managerProgress = 0;
        
        Vehicle TowTruck;
        Ped TowDriver;
        Vector3 PickupLocation;

        [Tick]
        private async Task OnInpoundTick()
        {
            if (VehToImpound == null) return;
#if DEBUG
            if (PlayerManager.IsDeveloper && TowTruck != null && PlayerManager.IsDeveloperUIActive)
                Screen.ShowSubtitle($"Dis: {Game.PlayerPed.Position.Distance(TowTruck.Position)} | prog: {managerProgress}");
#endif
            Sequence(managerProgress);
            await BaseScript.Delay(500);
        }

        async void Sequence(int progress)
        {
            if (VehToImpound == null)
                progress = 6;

            if (!VehToImpound.Exists())
                progress = 6;

            switch (progress)
            {
                case 0:
                    Logger.Log($"Clear if exists");
                    TowDriver?.Dismiss();
                    TowTruck?.Dismiss();
                    managerProgress = 1; // 1
                    break;
                case 1:
                    Logger.Log($"Create Tow Truck");
                    if (TowTruck != null)
                    {
                        managerProgress = 0;
                    }
                    TowTruck = await Vehicle.Spawn(VehicleHash.Flatbed, CalloutManager.ActiveCallout.Players[0].Character.Position.AroundStreet(200f, 400f));
                    TowDriver = await Ped.Spawn((PedHash)4154933561, TowTruck.Position, true);
                    TowDriver.IsImportant = true;

                    TowDriver.PutInVehicle(TowTruck);

                    TowTruck.Fx.IsSirenActive = true;

                    Blip blip = TowDriver.AttachBlip();
                    blip.Sprite = (BlipSprite)143;
                    blip.Color = BlipColor.TrevorOrange;
                    blip.Name = "Tow Truck Driver";

                    managerProgress = 2;
                    break;
                case 2:
                    Logger.Log($"Travel to Vehicle");
                    TaskSequence taskSequence = new TaskSequence();
                    taskSequence.AddTask.DriveTo(TowTruck.Fx, VehToImpound.Position, 10f, 30f, (int)CombinedVehicleDrivingFlags.Emergency);
                    TowDriver.Task.PerformSequence(taskSequence);
                    taskSequence.Close();

                    managerProgress = 3;
                    UiTools.Impound("Enroute...", "On our way, will be there soon!");
                    break;
                case 3:
                    if (TowDriver.Position.Distance(VehToImpound.Position) > 10f) return;

                    Logger.Log($"Leave Vehicle");

                    TaskSequence taskSequenceExit = new TaskSequence();
                    taskSequenceExit.AddTask.LeaveVehicle();
                    taskSequenceExit.AddTask.GoTo(VehToImpound.Position);
                    TowDriver.Task.PerformSequence(taskSequenceExit);
                    taskSequenceExit.Close();
                    managerProgress = 4;
                    break;
                case 4:
                    Logger.Log($"Wait till near vehicle");
                    if (TowDriver.Position.Distance(VehToImpound.Position) > 4f) return;
                    PickupLocation = TowTruck.Position;
                    managerProgress = 5;
                    break;
                case 5:
                    if (VehToImpound.IsAttachedTo(TowTruck)) return;
                    Logger.Log($"Attach vehicle to Truck");
                    VehToImpound.AttachTo(TowTruck.Bones[20], new Vector3(-0.5f, -5.0f, 1.0f));
                    TowTruck.Fx.IsSirenActive = false;
                    TowDriver.Task.CruiseWithVehicle(TowTruck.Fx, 30f, (int)CombinedVehicleDrivingFlags.Normal);
                    managerProgress = 6;
                    break;
                case 6:
                    if (PickupLocation.Distance(TowDriver.Position) < 50f) return;
                    Logger.Log($"Clean up");
                    TowDriver?.Dismiss();
                    TowTruck?.Dismiss();
                    VehToImpound?.Dismiss();

                    VehToImpound = null;
                    TowTruck = null;
                    TowDriver = null;

                    IsActive = false;
                    UiTools.Impound("Vehicle Impounded", "Vehicle has been impounded");
                    break;
            }
        }

        public static void Tow(Vehicle vehicle)
        {
            if (IsActive)
            {
                UiTools.Impound("Vehicle Impound", "We're currently enroute.");
                return;
            }

            managerProgress = 1;
            VehToImpound = vehicle;
            IsActive = true;
        }
    }
}
