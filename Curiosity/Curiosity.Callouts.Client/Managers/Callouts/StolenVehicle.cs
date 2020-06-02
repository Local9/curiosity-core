using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class StolenVehicle : Callout
    {
        private Ped criminal;
        private Vehicle stolenVehicle;
        private CalloutMessage calloutMessage = new CalloutMessage();

        List<VehicleHash> vehicleHashes = new List<VehicleHash>()
        {
            VehicleHash.Oracle2,
            VehicleHash.Panto,
            VehicleHash.Sandking,
            VehicleHash.SlamVan,
            VehicleHash.Adder,
            VehicleHash.Faggio,
            VehicleHash.Issi2,
            VehicleHash.Kuruma,
            VehicleHash.F620,
            VehicleHash.Dukes,
            VehicleHash.Baller,
            VehicleHash.Boxville,
            VehicleHash.Rumpo
        };

        List<PedHash> pedHashes = new List<PedHash>()
        {
            PedHash.Tourist01AFM,
            PedHash.Tourist01AFY,
            PedHash.Lost01GFY,
            PedHash.Lost01GMY,
        };

        List<WeaponHash> weaponHashes = new List<WeaponHash>()
        {
            WeaponHash.Pistol,
            WeaponHash.SMG,
            WeaponHash.MicroSMG
        };

        public StolenVehicle(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        /// <summary>
        /// Complete Requirements
        /// -- //
        /// Dead
        /// Arrested
        /// Distance
        /// -- //
        /// </summary>

        internal override async void Prepare()
        {
            base.Prepare();

            calloutMessage.CalloutType = CalloutType.STOLEN_VEHICLE;

            stolenVehicle = await Vehicle.Spawn(vehicleHashes.Random(),
                Players[0].Character.Position.AroundStreet(150f, 400f));
            RegisterVehicle(stolenVehicle);
            Logger.Log(stolenVehicle.Name);

            criminal = await Ped.Spawn(pedHashes.Random(), stolenVehicle.Position, true);

            criminal.IsPersistent = true;
            criminal.IsMission = true;

            if (Utility.RANDOM.Bool(0.8f))
            {
                criminal.Fx.RelationshipGroup = (uint)Collections.RelationshipHash.Prisoner;
                criminal.Fx.Weapons.Give(weaponHashes.Random(), 20, true, true);
            }

            RegisterPed(criminal);

            criminal.PutInVehicle(stolenVehicle);
            criminal.Task.CruiseWithVehicle(stolenVehicle.Fx, float.MaxValue,
                (int)Collections.CombinedVehicleDrivingFlags.Fleeing);
            criminal.AttachBlip(BlipColor.Red, true);
            PursuitManager.StartNewPursuit(this);

            stolenVehicle.IsSpikable = true;

            SendEmergancyNotification("CODE 3", "Stolen Vehicle");

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
                case 0:
                    calloutMessage.Name = Name;
                    calloutMessage.Success = true;
                    base.End();
                    break;
                case 1:
                    if (!criminal.IsDead) return; // FAILING
                    Logger.Log($"Ped is dead...");
                    progress++;
                    break;
                default:
                    End();
                    break;
            }
        }

        internal override void End(bool forcefully = false)
        {
            base.End(forcefully);

            base.CompleteCallout(calloutMessage);
        }
    }
}
