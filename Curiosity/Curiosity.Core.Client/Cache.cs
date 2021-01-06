using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Client
{
    public class Cache
    {
        public static CuriosityPlayer Player => CuriosityPlugin.Instance.Local;
        public static CuriosityEntity Entity => Player?.Entity;
        public static CuriosityCharacter Character => Player?.Character;
        public static Position Position => Entity.Position;
    }
}