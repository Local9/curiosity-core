using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterInfo
    {
        public uint Model;
        public string walkStyle;
        public string faceMood;
        public Dictionary<int, KeyValuePair<int, int>> Props = new Dictionary<int, KeyValuePair<int, int>>();
        public Dictionary<int, KeyValuePair<int, int>> DrawableVariations = new Dictionary<int, KeyValuePair<int, int>>();
    }
}