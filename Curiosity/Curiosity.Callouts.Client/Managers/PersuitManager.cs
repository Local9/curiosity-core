﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Classes;
using Curiosity.Callouts.Client.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ped = Curiosity.Callouts.Client.Classes.Ped;

namespace Curiosity.Callouts.Client.Managers
{
    public class PursuitManager : BaseScript
    {
        private static Pursuit activePursuit;
        public static bool IsPursuitActive => activePursuit != null;

        private static Sequence currentSqeunce = Sequence.REST;

        internal static void StartNewPursuit(Callout callout)
        {
            activePursuit = new Pursuit(callout);
            callout.Ended += OnCalloutEnded;
        }

        private static void OnCalloutEnded(bool forcefully)
        {
            activePursuit?.End();
            activePursuit = null;
        }

        [Tick]
        private async Task OnTick()
        {
            if (activePursuit == null)
            {
                if (activePursuit?.cops.Count > 0)
                {
                    activePursuit?.cops.ForEach(c =>
                    {
                        if (c.Item1.Exists())
                        {
                            c.Item1.Task.WanderAround();
                            c.Item1.MarkAsNoLongerNeeded();
                        }

                        if (c.Item2.Exists())
                            c.Item2.MarkAsNoLongerNeeded();
                    });
                }
                return;
            }

            foreach (Ped ped in activePursuit.Callout.RegisteredPeds)
            {
                if (ped.IsInVehicle != ped.Fx.IsInVehicle())
                {
                    ped.IsInVehicle = ped.Fx.IsInVehicle();
                }

                RedistributeCops(ped);
                await Delay(100);
            }
        }

        private void RedistributeCops(Ped ped)
        {
            if (activePursuit == null)
            {
                Logger.Log("No active pursuit running");
                return;
            }

            var nearestCops = activePursuit.cops
                .OrderBy(tuple => Vector3.Distance(tuple.Item1.Position, ped.Position))
                .ToArray();

            if (nearestCops.Length == 0) return;

            var alivePeds = activePursuit.Callout.RegisteredPeds
                .Where(registeredPed => !registeredPed.IsDead)
                .ToArray();

            if (alivePeds.Length == 0) return;

            int numRedistributedCops = nearestCops.Length / alivePeds.Length;

            for (var i = 0; i < numRedistributedCops; i++)
            {
                if (nearestCops[i].Item1.IsDead) // remove if dead
                {
                    CopSequence(Sequence.DEAD, ped, nearestCops[i]);
                }
                else if (ped.IsInVehicle)
                {
                    CopSequence(Sequence.CHASE, ped, nearestCops[i]);
                }
                else
                {
                    CopSequence(Sequence.ARREST, ped, nearestCops[i]);
                }
            }
        }

        internal enum Sequence
        {
            REST = 0,
            CHASE,
            ARREST,
            DEAD
        }

        internal static void CopSequence(Sequence sequence, Ped suspect, Tuple<Ped, Classes.Vehicle> officer)
        {
            if (currentSqeunce == sequence) return;
            currentSqeunce = sequence;

            Classes.Vehicle veh = officer.Item2;
            Ped cop = officer.Item1;

            switch(sequence)
            {
                case Sequence.REST:
                    break;
                case Sequence.CHASE:
                    if (!cop.IsInVehicle)
                        cop.PutInVehicle(veh);

                    cop.Task.VehicleChase(suspect.Fx);
                    break;
                case Sequence.ARREST:
                    cop.Task.Arrest(suspect.Fx);
                    break;
                case Sequence.DEAD:
                    cop.Delete();
                    veh.Delete();

                    activePursuit.cops.Remove(officer);
                    break;
            }
        }

        internal static async void AddNewCopToPursuit()
        {
            if (activePursuit.cops.Count > 0)
            {
                UiTools.Dispatch("Units Unavailable", "No more units are currently available");
                return;
            }

            Vector3 position = Game.PlayerPed.Position.AroundStreet(200f, 500f);
            var newCop = await SpawnManager.CreateNewCop(position);

            if (activePursuit != null) activePursuit.cops.Add(newCop);
            else
            {
                Logger.Log("Unable to add new cop. activePursuit was null.");
            }
        }
    }
}
