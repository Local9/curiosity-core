using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        static Ped Prisoner;
        static int managerProgress = 0;
        static int prisonerIndex = 0;

        Vehicle TransportVehicle;
        Ped TransportDriver;
        Vector3 PickupLocation;

        [Tick]
        private async Task OnPrisonerTransportClick()
        {
            if (Prisoner == null) return;

            CollectPrisoners(managerProgress);
            await BaseScript.Delay(500);
        }

        public static void Collect(Ped ped)
        {
            if (IsActive)
            {
                UiTools.Dispatch("Prison Transport", "We're currently enroute.");
                return;
            }

            managerProgress = 1;
            Prisoner = ped;
            IsActive = true;
        }

        private async void CollectPrisoners(int state)
        {
            if (Prisoner == null)
                state = 6;

            if (!Prisoner.Exists())
                state = 6;

            switch (state)
            {
                case 0:
                    TransportDriver?.Dismiss();
                    TransportVehicle?.Dismiss();
                    managerProgress = 1;
                    break;
                case 1:
                    if (TransportDriver != null)
                    {
                        managerProgress = 0;
                        return;
                    }
                    PedHash pedHash = (Utility.RANDOM.Next(2) == 1) ? PedHash.Cop01SFY : PedHash.Cop01SMY;

                    TransportVehicle = await Vehicle.Spawn(VehicleHash.Police2, CalloutManager.ActiveCallout.Players[0].Character.Position.AroundStreet(200f, 400f));
                    TransportDriver = await Ped.Spawn(pedHash, TransportVehicle.Position, true);

                    TransportDriver.Fx.Weapons.Give(WeaponHash.APPistol, 1, true, true);

                    TransportDriver.IsImportant = true;
                    TransportDriver.PutInVehicle(TransportVehicle);

                    TransportVehicle.Fx.IsSirenActive = true;

                    Blip blip = TransportDriver.AttachBlip();
                    blip.Sprite = (BlipSprite)143;
                    blip.Color = BlipColor.TrevorOrange;
                    blip.Name = "Prison Transport";

                    prisonerIndex = 0;

                    managerProgress = 2;

                    break;
                case 2:
                    Logger.Log($"Travel to position");
                    TaskSequence taskSequence = new TaskSequence();
                    taskSequence.AddTask.DriveTo(TransportVehicle.Fx, Prisoner.Position, 10f, 30f, (int)CombinedVehicleDrivingFlags.Emergency);
                    TransportDriver.Task.PerformSequence(taskSequence);
                    taskSequence.Close();

                    managerProgress = 3;
                    UiTools.Dispatch("Enroute...", $"On our way to collect the suspect.");
                    break;
                case 3:
                    if (TransportDriver.Position.Distance(Prisoner.Position) > 10f) return;

                    Logger.Log($"Leave Vehicle");

                    TransportVehicle.Fx.IsSirenActive = false;

                    TaskSequence taskSequenceExit = new TaskSequence();
                    taskSequenceExit.AddTask.LeaveVehicle();
                    taskSequenceExit.AddTask.GoTo(Prisoner.Position);
                    taskSequenceExit.AddTask.AimAt(Prisoner, -1);
                    TransportDriver.Task.PerformSequence(taskSequenceExit);
                    taskSequenceExit.Close();

                    managerProgress = 4;
                    break;
                case 4:
                    if (TransportDriver.Position.Distance(Prisoner.Position) > 1.5f) return;

                    Logger.Log($"Prisoner Leave Vehicle");

                    TaskSequence prisonerSequence = new TaskSequence();
                    TaskSequence escortPrisoner = new TaskSequence();

                    if (Prisoner.Fx.IsInVehicle())
                    {
                        prisonerSequence.AddTask.LeaveVehicle();
                    }

                    Prisoner.Task.EnterVehicle(TransportVehicle.Fx, VehicleSeat.LeftRear, 5000);
                    TransportDriver.Task.EnterVehicle(TransportVehicle.Fx, VehicleSeat.Driver, 5000);

                    PickupLocation = TransportVehicle.Position;

                    managerProgress = 5;
                    break;
                case 5:
                    if (!Prisoner.Fx.IsInVehicle()) return;
                    if (!TransportDriver.Fx.IsInVehicle()) return;

                    TransportDriver.Task.CruiseWithVehicle(TransportVehicle.Fx, 30f, (int)CombinedVehicleDrivingFlags.Normal);

                    managerProgress = 6;
                    break;
                case 6:
                    if (PickupLocation.Distance(TransportDriver.Position) < 50f) return;
                    Logger.Log($"Clean up");
                    TransportDriver?.Dismiss();
                    TransportVehicle?.Dismiss();
                    Prisoner?.Dismiss();

                    Prisoner = null;
                    TransportDriver = null;
                    TransportVehicle = null;

                    IsActive = false;
                    UiTools.Dispatch("Suspect collected", "");
                    managerProgress = 7;
                    break;
            }
        }
    }
}
