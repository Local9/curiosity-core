using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.Systems.Library.Events;

namespace Curiosity.MissionManager.Client.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("c:mm:damage", new AsyncEventCallback(async metadata =>
            {
                int networkId = metadata.Find<int>(0);
                float x = metadata.Find<float>(1);
                float y = metadata.Find<float>(2);
                float z = metadata.Find<float>(3);
                float force = metadata.Find<float>(4);
                float radius = metadata.Find<float>(5);
                bool fromEntity = metadata.Find<bool>(6);
                int numberOfHits = metadata.Find<int>(7);

                int handle = await EntityHandler.GetEntityFromNetworkId(networkId);

                for (var i = 0; i < numberOfHits; i++)
                    API.SetVehicleDamage(handle, x, y, z, force, radius, true);
                return null;
            }));

            EventSystem.Attach("c:mm:particle:location", new EventCallback(metadata =>
            {
                string dict = metadata.Find<string>(0);
                string fx = metadata.Find<string>(1);
                float x = metadata.Find<float>(2);
                float y = metadata.Find<float>(3);
                float z = metadata.Find<float>(4);
                float scale = metadata.Find<float>(5);
                bool placeOnGround = metadata.Find<bool>(6);

                Vector3 pos = new Vector3(x, y, z);
                float groundZ = 0f;

                if (API.GetGroundZFor_3dCoord(x, y, z, ref groundZ, false) && placeOnGround)
                    pos.Z = groundZ;

                ParticleEffectsAsset particleEffectsAsset = new ParticleEffectsAsset(dict);
                ParticleEffect particleEffect = particleEffectsAsset.CreateEffectAtCoord(fx, pos, scale: scale, startNow: true);

                Mission.RegisteredParticles.Add(particleEffect);

                return null;
            }));

            EventSystem.Attach("c:mm:particle:entity", new AsyncEventCallback(async metadata =>
            {
                int networkId = metadata.Find<int>(0);
                string dict = metadata.Find<string>(1);
                string fx = metadata.Find<string>(2);
                float x = metadata.Find<float>(3);
                float y = metadata.Find<float>(4);
                float z = metadata.Find<float>(5);
                float scale = metadata.Find<float>(6);

                int handle = await EntityHandler.GetEntityFromNetworkId(networkId);

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

                if (ent == null) return null;

                Vector3 pos = new Vector3(x, y, z);

                ParticleEffectsAsset particleEffectsAsset = new ParticleEffectsAsset(dict);
                EntityParticleEffect entityParticleEffect = particleEffectsAsset.CreateEffectOnEntity(fx, ent, pos, scale: scale, startNow: true);

                Mission.RegisteredParticles.Add(entityParticleEffect);

                return null;
            }));
        }
    }
}
