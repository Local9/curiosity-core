using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Server.net.Classes
{
    static class ServerSettings
    {
        public static bool IsWantedDisabled = false;
        public static int MaxWantedLevel = 5;

        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Settings:Wanted", new Action<Player>(WantedSettings));
        }

        static void WantedSettings([FromSource]Player player)
        {
            IsWantedDisabled = API.GetConvar("police_wanted_disabled", "false") == "true";
            MaxWantedLevel = API.GetConvarInt("police_max_wanted_level", 5);

            player.TriggerEvent("curiosity:Client:Settings:WantedDisabled", IsWantedDisabled, MaxWantedLevel);
        }
    }
}
