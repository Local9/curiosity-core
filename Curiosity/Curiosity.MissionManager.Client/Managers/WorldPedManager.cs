using CitizenFX.Core;
using Curiosity.MissionManager.Client.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
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

        public void Init()
        {
            Instance.AttachTickHandler(OnWorldPedList);
            Instance.AttachTickHandler(OnPedStunnedProtectionTick);
        }

        public void Dispose()
        {
            Instance.DetachTickHandler(OnWorldPedList);
            Instance.DetachTickHandler(OnPedStunnedProtectionTick);
        }


        private async Task OnWorldPedList()
        {
            try
            {
                foreach (KeyValuePair<int, Ped> kvp in WorldPeds.ToArray())
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

        private async Task OnPedStunnedProtectionTick()
        {
            try
            {
                SetWeaponDamageModifierThisFrame((uint)WeaponHash.StunGun, 0f);
                List<CitizenFX.Core.Ped> peds = World.GetAllPeds().Where(p => p.IsInRangeOf(Cache.PlayerPed.Position, 10f)).ToList();

                if (peds.Count == 0)
                {
                    await BaseScript.Delay(100);
                    return;
                }

                foreach(CitizenFX.Core.Ped ped in peds)
                {
                    if (ped.IsInVehicle()) continue;

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
                }
            }
            catch (Exception ex)
            {
                Cache.UpdatePedId();
                Logger.Error($"OnPedStunnedProtectionTick -> {ex}");
            }
        }
    }
}
