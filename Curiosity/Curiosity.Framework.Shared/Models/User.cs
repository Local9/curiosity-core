namespace Curiosity.Framework.Shared.Models
{
    [Serializable]
    public partial class User
    {
        public int Handle;
        public Player Player;
        public string Username;
        public List<Character> Characters = new();
    }
}
