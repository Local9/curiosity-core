using CitizenFX.Core;
using System;

namespace Curiosity.MissionManager.Server.Events
{
    public class ParticleEvents : BaseScript
    {
        public ParticleEvents()
        {
            EventHandlers["s:mm:particle:entity"] += new Action<int, string, string, float, float, float, float>(OnParticleEntity);
            EventHandlers["s:mm:particle:location"] += new Action<string, string, float, float, float, float, bool>(OnParticleLocation);
        }

        private void OnParticleLocation(string dict, string fx, float x, float y, float z, float scale, bool placeOnGround)
        {
            BaseScript.TriggerClientEvent("c:mm:particle:location", dict, fx, x, y, z, scale, placeOnGround);
        }

        private void OnParticleEntity(int vehNetworkId, string dict, string fx, float x, float y, float z, float scale)
        {
            BaseScript.TriggerClientEvent("c:mm:particle:entity", vehNetworkId, dict, fx, x, y, z, scale);
        }
    }
}
