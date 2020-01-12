using Curiosity.System.Client.Environment.Entities;
using Curiosity.System.Library.Models;

namespace Curiosity.System.Client
{
    public class Cache
    {
        public static CuriosityPlayer Player => CuriosityPlugin.Instance.Local;
        public static CuriosityEntity Entity => Player?.Entity;
        public static CuriosityCharacter Character => Player?.Character;
        public static Position Position => Entity.Position;
    }
}