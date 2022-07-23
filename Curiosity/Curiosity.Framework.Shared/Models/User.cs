using Lusive.Events.Attributes;

namespace Curiosity.Framework.Shared.Models
{
    [Serializable]
    public partial class User
    {
        public int Handle;

        [Ignore]
        public Player Player;

        public string Username;
        public List<Character> Characters = new();

        
    }
}
