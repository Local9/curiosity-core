namespace Curiosity.Framework.Client.Managers.GameWorld.Entity
{
    internal class WorldVehicle
    {
        public Vehicle Vehicle { get; }
        public PluginManager Instance => PluginManager.Instance;

        public WorldVehicle(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }

        public virtual void Dispose()
        {
            Vehicle?.Delete();
        }
    }
}
