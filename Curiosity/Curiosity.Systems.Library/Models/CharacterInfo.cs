using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterInfo
    {
        public uint Model;
        public Dictionary<int, int> Props = new Dictionary<int, int>();
        public Dictionary<int, int> PropTextures = new Dictionary<int, int>();
        public Dictionary<int, int> DrawableVariations = new Dictionary<int, int>();
        public Dictionary<int, int> DrawableVariationTextures = new Dictionary<int, int>();
    }
}