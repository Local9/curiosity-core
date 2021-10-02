using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterTattoos
    {
        public List<KeyValuePair<string, string>> TorsoTattoos = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> HeadTattoos = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> LeftArmTattoos = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> RightArmTattoos = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> LeftLegTattoos = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> RightLegTattoos = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> BadgeTattoos = new List<KeyValuePair<string, string>>();
    }
}
