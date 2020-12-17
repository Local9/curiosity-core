using CitizenFX.Core;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.MissionManager.Client.Managers
{
    public class WorldPedManager : Manager<WorldPedManager>
    {
        ConcurrentDictionary<int, Ped> WorldPeds = new ConcurrentDictionary<int, Ped>();

        public override void Begin()
        {
            Logger.Info($"- [WorldPedManager] Begin ------------------------");
        }

        [TickHandler]
        private async Task OnWorldPedList()
        {
            ConcurrentDictionary<int, Ped> WorldPedsCopy = WorldPeds;

            foreach(KeyValuePair<int, Ped> kvp in WorldPedsCopy)
            {
                Ped ped = kvp.Value;

                if (!ped.Exists())
                {
                    WorldPeds.TryRemove(kvp.Key, out Ped old);
                }
                else if (ped.DateCreated.Subtract(DateTime.Now).TotalSeconds > 60 && !ped.IsMission)
                {
                    ped.Dismiss();
                    WorldPeds.TryRemove(kvp.Key, out Ped old);
                }
            }

            DateTime startPolling = DateTime.Now;
            while (startPolling.Subtract(DateTime.Now).TotalSeconds < 10)
            {
                await BaseScript.Delay(1000);
            }
        }

        [TickHandler]
        private async Task OnPedStunnedProtectionTick()
        {
            try
            {
                List<CitizenFX.Core.Ped> peds = World.GetAllPeds().Where(p => p.IsInRangeOf(Game.PlayerPed.Position, 30f)).ToList();

                if (peds.Count == 0)
                {
                    await BaseScript.Delay(100);
                    return;
                }

                peds.ForEach(async ped =>
                {
                    if (WorldPeds.ContainsKey(ped.Handle)) return;

                    bool setup = Decorators.GetBoolean(ped.Handle, Decorators.PED_SETUP);

                    if (!setup && !ped.IsPlayer)
                    {
                        if (ped.IsBeingStunned)
                        {
                            Ped curPed = new Ped(ped, false, true);
                            curPed.IsImportant = false;
                            curPed.IsMission = false;
                            curPed.IsSuspect = false;
                            curPed.IsArrestable = false;

                            WorldPeds.TryAdd(curPed.Handle, curPed);
                        }
                    }

                    await BaseScript.Delay(100);

                    // NativeWrapper.Draw3DText(ped.Position.X, ped.Position.Y, ped.Position.Z, $"A: {ped.IsAlive}, H: {ped.Health}, S: {ped.IsBeingStunned}", 40f, 15f);
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"OnPedStunnedProtectionTick -> {ex}");
            }
        }
    }
}
