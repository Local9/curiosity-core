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
            if (Cache.PlayerPed.IsReloading)
            {
                if (isReloadingCheck) return;
                
                int ammoCount = Cache.PlayerPed.Weapons.Current.Ammo;

                Logger.Debug($"Player is Reloading, current ammo {ammoCount}");
            }
            else
            {
                isReloadingCheck = false;
            }
        }
    }
}
