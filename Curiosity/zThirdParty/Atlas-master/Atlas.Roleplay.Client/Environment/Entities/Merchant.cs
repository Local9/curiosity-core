using Atlas.Roleplay.Client.Environment.Entities.Modules.Impl;

namespace Atlas.Roleplay.Client.Environment.Entities
{
    public class Merchant : AtlasEntity
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