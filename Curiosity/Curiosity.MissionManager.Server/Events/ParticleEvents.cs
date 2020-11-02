using CitizenFX.Core;
using System;

namespace Curiosity.MissionManager.Server.Events
{
    public class ParticleEvents : BaseScript
    {
        public ParticleEvents()
        {
            EventHandlers["s:mm:particle"] += new Action<int, string, string, float, float, float, float>(OnParticle); // MOVE
        }

        private void OnParticle(int vehNetworkId, string dict, string fx, float x, float y, float z, float scale)
        {
            BaseScript.TriggerClientEvent("c:mm:particle", vehNetworkId, dict, fx, x, y, z, scale);
        }
    }
}
