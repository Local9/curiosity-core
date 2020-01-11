using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment.Entities.Modules.Impl
{
    public class EntityNetworkModule : EntityModule
    {
        protected override void Begin(AtlasEntity entity, int id)
        {
            if (!API.NetworkGetEntityIsNetworked(id)) API.NetworkRegisterEntityAsNetworked(id);
        }

        public bool IsClaimed()
        {
            return API.NetworkHasControlOfEntity(Id);
        }

        public async Task Claim()
        {
            API.NetworkRequestControlOfEntity(Id);

            while (!IsClaimed())
            {
                await BaseScript.Delay(100);
            }
        }

        public int GetId()
        {
            return API.NetworkGetNetworkIdFromEntity(Id);
        }

        public static AtlasEntity GetEntity(int id)
        {
            var entity = new AtlasEntity(API.NetworkGetEntityFromNetworkId(id));
            
            entity.InstallModule("Network", new EntityNetworkModule());

            return entity;
        }
    }
}