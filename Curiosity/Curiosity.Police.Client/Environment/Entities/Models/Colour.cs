
namespace Curiosity.Police.Client.Environment.Entities.Models
{
    internal class Colour
    {
        public int r;
        public int g;
        public int b;

        public Colour(int r, int g, int b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Colour(int x)
        {
            this.r = x;
            this.g = x;
            this.b = x;
        }
    }
}
