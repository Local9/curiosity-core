using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Menu.Submenu;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Players[0].Character.Position.AroundStreet(200f, 400f));
            RegisterVehicle(stolenVehicle);
            Logger.Log(stolenVehicle.Hash);

            // CONSIDER ESCAPE

            criminal = await Ped.Spawn(pedHashes.Random(), stolenVehicle.Position, true);

            criminal.IsPersistent = true;
            criminal.IsImportant = true;
            criminal.IsMission = true;

            if (Utility.RANDOM.Bool(0.8f))
            {
                criminal.Fx.RelationshipGroup = (uint)Collections.RelationshipHash.Prisoner;
                criminal.Fx.RelationshipGroup.SetRelationshipBetweenGroups(Game.PlayerPed.RelationshipGroup, Relationship.Hate);
                criminal.Fx.Weapons.Give(weaponHashes.Random(), 20, true, true);
            }

            RegisterPed(criminal);

            criminal.PutInVehicle(stolenVehicle);
            criminal.Task.CruiseWithVehicle(stolenVehicle.Fx, float.MaxValue,
                (int)Collections.CombinedVehicleDrivingFlags.Fleeing);
            criminal.AttachBlip(BlipColor.Red, true);
            PursuitManager.StartNewPursuit(this);

            stolenVehicle.IsSpikable = true;
            stolenVehicle.IsMission = true;
            stolenVehicle.IsTowable = true;

            UiTools.Dispatch("~r~CODE 3~s~: Stolen Vehicle", $"~b~Make~s~: {stolenVehicle.Name}~n~~b~Plate~s~: {stolenVehicle.Fx.Mods.LicensePlate}");

            base.IsSetup = true;
        }

        internal async override void Tick()
        {
            if (criminal == null && !IsRunning)
            {
                Logger.Log("Criminal was null");
                End(true);
                return;
            }
            IsRunning = true;

            float roll = API.GetEntityRoll(stolenVehicle.Fx.Handle);
            if ((roll > 75.0f || roll < -75.0f) && stolenVehicle.Fx.Speed < 2f)
            {
                TaskSequence taskSequence = new TaskSequence();
                taskSequence.AddTask.LeaveVehicle(LeaveVehicleFlags.BailOut);
                taskSequence.AddTask.FleeFrom(Game.PlayerPed);
                criminal.Task.PerformSequence(taskSequence);
                taskSequence.Close();
            }

            if (stolenVehicle.Fx.Speed < 4.0f && criminal.IsInVehicle)
            {
                long gameTimer = API.GetGameTimer();
                Vector3 location = stolenVehicle.Position;
                while (criminal.IsInVehicle && stolenVehicle.Position.Distance(location) < 4.0f)
                {
                    await BaseScript.Delay(100);
                    if ((API.GetGameTimer() - gameTimer) > 10000)
                    {
                        TaskSequence fleeVehicle = new TaskSequence();
                        fleeVehicle.AddTask.LeaveVehicle(LeaveVehicleFlags.BailOut);
                        fleeVehicle.AddTask.FleeFrom(Game.PlayerPed);
                        criminal.Task.PerformSequence(fleeVehicle);
                        fleeVehicle.Close();
                    }
                }
            }

            int numberOfAlivePlayers = Players.Select(x => x).Where(x => x.IsAlive).Count();

            if (numberOfAlivePlayers == 0) // clear callout
            {
                End(true);
            }

            if (criminal.Position.Distance(Game.PlayerPed.Position) > 600f)
            {
                UiTools.Dispatch("~r~Pursuit Ended", "Suspect has got away. Return to patrol.");
                End(true);
            }
        }

        internal override void End(bool forcefully = false, CalloutMessage cm = null)
        {
            cm = calloutMessage;
            base.End(forcefully, cm);
        }
    }
}
