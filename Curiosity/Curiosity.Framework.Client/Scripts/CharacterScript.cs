using Curiosity.Framework.Shared.Models;

namespace Curiosity.Framework.Client.Scripts
{
    internal class CharacterScript
    {
        static User _user = new();

        public static void Init()
        {
            _user.Sound.Disable();
        }
    }
}
