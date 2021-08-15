using CitizenFX.Core;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.State;
using Curiosity.Systems.Library.Models;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client
{
    public class Cache
    {
        public static CuriosityPlayer Player => PluginManager.Instance.Local;
        public static CuriosityEntity Entity => Player?.Entity;
        public static CuriosityCharacter Character => Player?.Character;
        public static Position Position => Entity.Position;
        // Vehicles
        public static VehicleState PersonalVehicle;
        public static VehicleState PersonalTrailer;
        public static VehicleState PersonalBoat;
        public static VehicleState PersonalPlane;
        public static VehicleState PersonalHelicopter;

        private static Ped _ped;
        public static Ped PlayerPed
        {
            get
            {
                if (_ped == null)
                    UpdatePedId();

                if (_ped.Handle != PlayerPedId())
                    UpdatePedId();

                return _ped;
            }
            internal set => _ped = value;
        }

        static Cache()
        {
            _ped = new Ped(PlayerPedId());
        }

        public static void UpdatePedId(bool export = false)
        {
            PlayerPed = new Ped(PlayerPedId());
            _ped = PlayerPed;


            if (export)
            {
                PluginManager.Instance.ExportDictionary["curiosity-missions"].RefreshPlayerPed();
            }
        }
    }
}