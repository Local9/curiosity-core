using CitizenFX.Core;
using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.Systems.Library.Models;
using static CitizenFX.Core.Native.API;

namespace Curiosity.MissionManager.Client
{
    public class Cache
    {
        public static CuriosityPlayer Player => PluginManager.Instance.Local;
        public static CuriosityEntity Entity => Player?.Entity;
        public static CuriosityCharacter Character => Player?.Character;
        public static Position Position => Entity.Position;

        private static Ped ped;
        public static Ped PlayerPed
        {
            get => ped;
            internal set => ped = value;
        }

        public static void UpdatePedId() => PlayerPed = new Ped(PlayerPedId());
    }
}