using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Handler
{
    public class ParticleHandler : BaseScript
    {
        public ParticleHandler()
        {
            EventHandlers["c:mm:particle"] += new Action<int, string, string, float, float, float, float>(OnParticleEvent);
        }

        private void OnParticleEvent(int vehNetworkId, string dict, string fx, float x, float y, float z, float scale)
        {
            if (!API.NetworkDoesNetworkIdExist(vehNetworkId)) return;

            Vector3 pos = new Vector3(x, y, z);

            int vehHandle = API.NetworkGetEntityFromNetworkId(vehNetworkId);

            if (!API.DoesEntityExist(vehHandle)) return;

            Vehicle vehicle = new Vehicle(vehHandle);

            ParticleEffectsAsset particleEffectsAsset = new ParticleEffectsAsset(dict);
            particleEffectsAsset.CreateEffectOnEntity(fx, vehicle, pos, scale: scale, startNow: true);
        }
    }
}
