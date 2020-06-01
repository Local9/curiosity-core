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
            VehicleHash.SlamVan
        };

        List<PedHash> pedHashes = new List<PedHash>()
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

            base.Name = "Stolen Vehicle";

            stolenVehicle = await Vehicle.Spawn(vehicleHashes.Random(),
                Players[0].Character.Position.AroundStreet(250f, 600f));
            RegisterVehicle(stolenVehicle);
            Logger.Log(stolenVehicle.Name);

            criminal = await Ped.Spawn(pedHashes.Random(), stolenVehicle.Position);

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

            float distance = Game.PlayerPed.Position.Distance(criminal.Position);
            Screen.ShowSubtitle($"Distance: {distance}");

            if (distance > 500 && distance < 800)
            {
                Screen.ShowSubtitle($"~o~WARNING~s~: Suspect is getting away!");
            }

            if (distance > 850)
            {
                Screen.ShowSubtitle($"~r~Suspect has got away! Get'em next time.");
                End();
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
                    if (!criminal.IsDead) return;
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
