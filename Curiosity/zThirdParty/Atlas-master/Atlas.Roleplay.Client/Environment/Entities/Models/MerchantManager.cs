using Atlas.Roleplay.Client.Environment.Entities.Modules.Impl;
using Atlas.Roleplay.Client.Managers;

namespace Atlas.Roleplay.Client.Environment.Entities.Models
{
    public class MerchantManager : Manager<MerchantManager>
    {
        public Merchant GetMerchantById(int id)
        {
            return new Merchant(EntityNetworkModule.GetEntity(id).Id);
        }
    }
}