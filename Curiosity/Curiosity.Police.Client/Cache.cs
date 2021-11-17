using CitizenFX.Core;
using Curiosity.Police.Client.Environment.Entities;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client
{
    public class Cache
    {
        private static CuriosityPlayer _curiosityPlayer;

        public static Player Player => new Player(GetPlayerPed(PlayerId()));
        public static CuriosityPlayer CuriosityPlayer
        { 
            get
            {
                if (_curiosityPlayer is not null)
                    return _curiosityPlayer;

                return _curiosityPlayer = new CuriosityPlayer();
            } 
        }
    }
}