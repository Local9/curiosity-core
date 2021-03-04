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
        public static Vehicle PersonalVehicle { get; internal set; }

        private static Ped _ped = null;
        public static Ped PlayerPed
        {
            get
            {
                if (_ped == null)
                    UpdatePedId();

                return _ped;
            }
            internal set => _ped = value;
        }

        static Cache()
        {
            _ped = new Ped(PlayerPedId());
        }

        public static void UpdatePedId() => PlayerPed = new Ped(PlayerPedId());
    }
}