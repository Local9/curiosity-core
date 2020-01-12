using Curiosity.System.Client.Environment.Entities.Modules.Impl;
using Curiosity.System.Client.Managers;

namespace Curiosity.System.Client.Environment.Entities.Models
{
    public class MerchantManager : Manager<MerchantManager>
    {
        public Merchant GetMerchantById(int id)
        {
            return new Merchant(EntityNetworkModule.GetEntity(id).Id);
        }
    }
}