namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class SaleSign
    {
        public string Model { get; set; }
        public Quaternion Position { get; set; }

        public SaleSign(string model, Quaternion position)
        {
            Model = model;
            Position = position;
        }
    }
}
