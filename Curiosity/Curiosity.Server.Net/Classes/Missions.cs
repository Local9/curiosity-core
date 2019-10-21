using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Server.net.Helpers;
using CitizenFX.Core;

namespace Curiosity.Server.net.Classes
{
    class Missions
    {
        static Server server = Server.GetInstance();
        static Random random = new Random();

        public static void Init()
        {
            // Player who triggered it
            // Player to update
            // Is mission ped
            // increase or decrease

            server.RegisterEventHandler("curiosity:Server:Missions:KilledPed", new Action<CitizenFX.Core.Player, string, string, bool, bool>(OnKilledPed));

        }


        static void OnKilledPed([FromSource]CitizenFX.Core.Player player, string source, string skill, bool missionPed, bool increase)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{source}"))
            {
                Log.Error($"IncreaseSkill: Player session missing.");
                return;
            }

            if (increase)
            {
                Skills.IncreaseSkill(source, skill, random.Next(5, 10));
                Skills.IncreaseSkill(source, "knowledge", random.Next(3, 6));
            }

            if (!increase)
            {
                Skills.DecreaseSkill(source, skill, 5);
                Skills.DecreaseSkill(source, "knowledge", 3);
            }
        }
    }
}
