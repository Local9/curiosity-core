using Curiosity.Core.Client.Diagnostics;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class WeaponManager : Manager<WeaponManager>
    {
        bool isReloadingCheck = false;

        public override void Begin()
        {

        }

        [TickHandler(SessionWait = true)]
        private async Task OnReloadTick()
        {
            if (Cache.PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed) return;

            if (Cache.PlayerPed.IsReloading)
            {
                if (isReloadingCheck) return;
                isReloadingCheck = true;

                int pedHandle = Cache.PlayerPed.Handle;

                uint weaponHash = 0;

                if (API.GetCurrentPedWeapon(pedHandle, ref weaponHash, true))
                {
                    int ammoCount = API.GetAmmoInPedWeapon(pedHandle, weaponHash);

                    Logger.Debug($"Player is Reloading, current {weaponHash} TotalAmmo {ammoCount}");
                }
            }
            else
            {
                isReloadingCheck = false;
            }
        }
    }
}
