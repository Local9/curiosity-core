using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Environment.Entities.Modules.Impl
{
    public class EntityNetworkModule : EntityModule
    {
        protected override void Begin(CuriosityEntity entity, int id)
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

        public static CuriosityEntity GetEntity(int id)
        {
            var entity = new CuriosityEntity(API.NetworkGetEntityFromNetworkId(id));

            entity.InstallModule("Network", new EntityNetworkModule());

            return entity;
        }
    }
}