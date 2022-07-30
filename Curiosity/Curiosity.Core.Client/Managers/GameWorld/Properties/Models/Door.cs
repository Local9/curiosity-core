namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class Door
    {
        private int _hash;
        public string Model { get; set; }
        public Vector3 Position { get; set; }

        public Door(string model, Vector3 position)
        {
            Model = model;
            Position = position;
            _hash = GetHashKey(model);

            AddDoorToSystem((uint)_hash, (uint)_hash, position.X, position.Y, position.Z, false, true, true);
            Lock();
        }

        public Door(int model, Vector3 position)
        {
            Position = position;
            _hash = model;

            AddDoorToSystem((uint)_hash, (uint)_hash, position.X, position.Y, position.Z, false, true, true);
            Lock();
        }

        public void Lock()
        {
            DoorSystemSetDoorState((uint)_hash, 1, true, true);
        }

        public void Unlock()
        {
            DoorSystemSetDoorState((uint)_hash, 0, true, true);
        }
    }
}
