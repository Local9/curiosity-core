namespace Curiosity.Core.Client.Environment.Entities.Models
{
    public class Vector5
    {
        public Vector3 Vector3 { get; set; }
        public Vector2 Vector2 { get; set; }

        public Vector5(Vector3 vector3, Vector2 vector2)
        {
            Vector3 = vector3;
            Vector2 = vector2;
        }

        public static Vector5 Zero => new Vector5(Vector3.Zero, Vector2.Zero);
    }
}
