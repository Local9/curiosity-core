using Curiosity.System.Client.Environment.Entities.Modules.Impl;

namespace Curiosity.System.Client.Environment.Entities
{
    public class Merchant : CuriosityEntity
    {
        public EntityDecorModule Decors => (EntityDecorModule)Modules["Decors"];
        public EntityNetworkModule NetworkModule => (EntityNetworkModule)Modules["Network"];

        public Merchant(int id) : base(id)
        {
            InstallModule("Decors", new EntityDecorModule());
            InstallModule("Network", new EntityNetworkModule());
        }
    }
}