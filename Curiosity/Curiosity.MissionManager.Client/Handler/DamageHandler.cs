using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.MissionManager.Client.Handler
{
    public class DamageHandler : BaseScript
    {
        public DamageHandler()
        {
            EventHandlers["c:mm:damage"] += new Action<int, float, float, float, float, float, bool, int>(OnApplyDamage);
        }

        private void OnApplyDamage(int networkId, float x, float y, float z, float force, float radius, bool fromEntity, int numberOfHits)
        {
            int handle = EntityHandler.GetEntityFromNetworkId(networkId);

            for (var i = 0; i < numberOfHits; i++)
                API.SetVehicleDamage(handle, x, y, z, force, radius, true);
        }
    }
}
