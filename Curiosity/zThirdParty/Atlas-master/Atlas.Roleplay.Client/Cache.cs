using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Client
{
    public class Cache
    {
        public static AtlasPlayer Player => AtlasPlugin.Instance.Local;
        public static AtlasEntity Entity => Player?.Entity;
        public static AtlasCharacter Character => Player?.Character;
        public static Position Position => Entity.Position;
    }
}