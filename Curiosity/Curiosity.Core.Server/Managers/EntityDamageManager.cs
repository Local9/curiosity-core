using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Server.Managers
{
    public class EntityDamageManager : Manager<EntityDamageManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("s:mm:damage", new EventCallback(metadata =>
            {
                int networkId = metadata.Find<int>(0);
                float x = metadata.Find<float>(1);
                float y = metadata.Find<float>(2);
                float z = metadata.Find<float>(3);
                float force = metadata.Find<float>(4);
                float radius = metadata.Find<float>(5);
                bool fromEntity = metadata.Find<bool>(6);
                int numberOfHits = metadata.Find<int>(7);

                EventSystem.SendAll("c:mm:damage", networkId, x, y, z, force, radius, fromEntity, numberOfHits);
                return null;
            }));
        }
    }
}
