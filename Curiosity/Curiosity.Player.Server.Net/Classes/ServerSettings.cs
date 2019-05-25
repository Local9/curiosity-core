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
        public static bool IsInstantRefuelDisabled = false;
        public static int MaxWantedLevel = 5;

        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Settings:Wanted", new Action<Player>(WantedSettings));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Settings:InstantRefuel", new Action<Player>(InstantRefuel));

            IsWantedDisabled = API.GetConvar("police_wanted_disabled", "false") == "true";
            MaxWantedLevel = API.GetConvarInt("police_max_wanted_level", 5);
            IsInstantRefuelDisabled = API.GetConvar("instant_refuel_disabled", "false") == "true";

            Log.Verbose($"ServerSettings -> IsWantedDisabled {IsWantedDisabled}");
            Log.Verbose($"ServerSettings -> MaxWantedLevel {MaxWantedLevel}");
            Log.Verbose($"ServerSettings -> IsInstantRefuelDisabled {IsInstantRefuelDisabled}");

        }

        static void WantedSettings([FromSource]Player player)
        {
            IsWantedDisabled = API.GetConvar("police_wanted_disabled", "false") == "true";
            MaxWantedLevel = API.GetConvarInt("police_max_wanted_level", 5);
            player.TriggerEvent("curiosity:Client:Settings:WantedDisabled", IsWantedDisabled, MaxWantedLevel);
        }

        static void InstantRefuel([FromSource]Player player)
        {
            IsInstantRefuelDisabled = API.GetConvar("instant_refuel_disabled", "false") == "true";
            player.TriggerEvent("curiosity:Client:Settings:InstantRefuel", IsInstantRefuelDisabled);
        }
    }
}
