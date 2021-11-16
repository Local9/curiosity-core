using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Server.Managers
{
    public class ParticleManager : Manager<ParticleManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("s:mm:particle:location", new EventCallback(metadata =>
            {
                string dict = metadata.Find<string>(0);
                string fx = metadata.Find<string>(1);
                float x = metadata.Find<float>(2);
                float y = metadata.Find<float>(3);
                float z = metadata.Find<float>(4);
                float scale = metadata.Find<float>(5);
                bool placeOnGround = metadata.Find<bool>(6);

                EventSystem.SendAll("c:mm:particle:location", dict, fx, x, y, z, scale, placeOnGround);
                return null;
            }));

            EventSystem.Attach("s:mm:particle:entity", new EventCallback(metadata =>
            {
                int networkId = metadata.Find<int>(0);
                string dict = metadata.Find<string>(1);
                string fx = metadata.Find<string>(2);
                float x = metadata.Find<float>(3);
                float y = metadata.Find<float>(4);
                float z = metadata.Find<float>(5);
                float scale = metadata.Find<float>(6);

                EventSystem.SendAll("c:mm:particle:entity", networkId, dict, fx, x, y, z, scale);
                return null;
            }));
        }
    }
}
