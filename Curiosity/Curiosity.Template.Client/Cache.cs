using Curiosity.Systems.Library.Models;
using Curiosity.Template.Client.Environment.Entities;

namespace Curiosity.Template.Client
{
    public class Cache
    {
        public static CuriosityPlayer Player => CuriosityPlugin.Instance.Local;
        public static CuriosityEntity Entity => Player?.Entity;
        public static CuriosityCharacter Character => Player?.Character;
        public static Position Position => Entity.Position;
    }
}