namespace Curiosity.Core.Client.Environment.Entities.Models
{
    internal class Vector5
    {
        public Vector3 Vector3 { get; set; }
        public Vector2 Vector2 { get; set; }

        public Vector5(Vector3 vector3, Vector2 vector2)
        {
            Vector3 = vector3;
            Vector2 = vector2;
        }
    }
}
