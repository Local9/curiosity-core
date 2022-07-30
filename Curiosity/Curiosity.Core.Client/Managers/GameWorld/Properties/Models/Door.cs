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

        }

        public void Unlock()
        {

        }
    }
}
