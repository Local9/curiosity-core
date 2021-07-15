using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Events;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Handler
{
    public class EntityHandler
    {
        public static async Task<int> GetEntityFromNetworkId(int networkId)
        {
            if (!API.NetworkDoesNetworkIdExist(networkId)) return 0;

            int handle = API.NetworkGetEntityFromNetworkId(networkId);

            if (!API.DoesEntityExist(handle)) return 0;

            while (!API.NetworkHasControlOfNetworkId(networkId))
            {
                await BaseScript.Delay(0);
                API.NetworkRequestControlOfNetworkId(networkId);
            }

            return handle;
        }

        public static void TriggerDamageEvent(int networkId, float x, float y, float z, float force, float radius, bool towardsEntity, int numberOfHits)
        {
            EventSystem.GetModule().Send("s:mm:damage", networkId, x, y, z, force, radius, towardsEntity, numberOfHits);
        }

        public static void ParticleEffect(int networkId, string dict, string fx, Vector3 offset, float scale)
        {
            EventSystem.GetModule().Send("s:mm:particle:entity", networkId, dict, fx, offset.X, offset.Y, offset.Z, scale);
        }
    }
}
