using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.MissionManager.Client.Handler
{
    public class ParticleHandler : BaseScript
    {
        public ParticleHandler()
        {
            EventHandlers["c:mm:particle:entity"] += new Action<int, string, string, float, float, float, float>(OnParticleEventEntity);
            EventHandlers["c:mm:particle:location"] += new Action<string, string, float, float, float, float, bool>(OnParticleEventLocation);
        }

        private void OnParticleEventEntity(int networkId, string dict, string fx, float x, float y, float z, float scale)
        {
            if (!Mission.isOnMission) return;

            int handle = EntityHandler.GetEntityFromNetworkId(networkId);

            Entity ent = null;

            switch (API.GetEntityType(handle))
            {
                case 1:
                    ent = new Ped(handle);
                    break;
                case 2:
                    ent = new Vehicle(handle);
                    break;
            }

            if (ent == null) return;

            Vector3 pos = new Vector3(x, y, z);

            ParticleEffectsAsset particleEffectsAsset = new ParticleEffectsAsset(dict);
            EntityParticleEffect entityParticleEffect = particleEffectsAsset.CreateEffectOnEntity(fx, ent, pos, scale: scale, startNow: true);

            Mission.RegisteredParticles.Add(entityParticleEffect);
        }

        private void OnParticleEventLocation(string dict, string fx, float x, float y, float z, float scale, bool placeOnGround)
        {
            if (!Mission.isOnMission) return;

            Vector3 pos = new Vector3(x, y, z);
            float groundZ = 0f;

            if (API.GetGroundZFor_3dCoord(x, y, z, ref groundZ, false) && placeOnGround)
                pos.Z = groundZ;

            ParticleEffectsAsset particleEffectsAsset = new ParticleEffectsAsset(dict);
            ParticleEffect particleEffect = particleEffectsAsset.CreateEffectAtCoord(fx, pos, scale: scale, startNow: true);
            
            Mission.RegisteredParticles.Add(particleEffect);
        }
    }
}
