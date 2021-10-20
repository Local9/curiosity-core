using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
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

        [TickHandler] // TODO: Maybe move this also into "Job Start"
        private async Task OnWorldPedList()
        {
            try
            {
                ConcurrentDictionary<int, Ped> WorldPedsCopy = WorldPeds;

                foreach (KeyValuePair<int, Ped> kvp in WorldPedsCopy)
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
            catch (Exception ex)
            {
                Cache.UpdatePedId();
                Logger.Error($"OnWorldPedList -> {ex}");
            }
        }

        [TickHandler] // TODO: Maybe move this also into "Job Start"
        private async Task OnPedStunnedProtectionTick()
        {
            try
            {
                SetWeaponDamageModifierThisFrame((uint)WeaponHash.StunGun, 0f);
                List<CitizenFX.Core.Ped> peds = World.GetAllPeds().Where(p => p.IsInRangeOf(Cache.PlayerPed.Position, 30f)).ToList();

                if (peds.Count == 0)
                {
                    await BaseScript.Delay(100);
                    return;
                }

                peds.ForEach(ped =>
                {
                    if (!IsEntityStatic(ped.Handle))
                    {
                        ped.CanWrithe = false;

                        if (ped.IsBeingStunned)
                        {
                            ped.DropsWeaponsOnDeath = false;
                            // ped.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_DieWhenRagdoll, false);

                            ped.Health = ped.MaxHealth;
                            ped.ClearBloodDamage();
                        }

                        if (ped.IsInjured)
                        {
                            ReviveInjuredPed(ped.Handle);
                        }
                    }

                    // NativeWrapper.Draw3DText(ped.Position.X, ped.Position.Y, ped.Position.Z, $"A: {ped.IsAlive}, H: {ped.Health}, S: {ped.IsBeingStunned}", 40f, 15f);
                });
            }
            catch (Exception ex)
            {
                Cache.UpdatePedId();
                Logger.Error($"OnPedStunnedProtectionTick -> {ex}");
            }
        }
    }
}
