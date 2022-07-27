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
        }

        public void Lock()
        {
            DoorControl((uint)_hash, Position.X, Position.Y, Position.Z, true, 0f, 50f, 0f);
        }

        public void Unlock()
        {
            DoorControl((uint)_hash, Position.X, Position.Y, Position.Z, false, 0f, 50f, 0f);
        }
    }
}
