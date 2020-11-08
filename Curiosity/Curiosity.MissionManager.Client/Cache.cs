using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.Systems.Library.Models;

namespace Curiosity.MissionManager.Client
{
    public class Cache
    {
        public static CuriosityPlayer Player => PluginManager.Instance.Local;
        public static CuriosityEntity Entity => Player?.Entity;
        public static CuriosityCharacter Character => Player?.Character;
        public static Position Position => Entity.Position;
    }
}