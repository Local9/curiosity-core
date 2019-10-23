using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Server.net.Helpers;
using CitizenFX.Core;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Newtonsoft.Json;

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

            server.RegisterEventHandler("curiosity:Server:Missions:KilledPed", new Action<CitizenFX.Core.Player, string>(OnKilledPed));
            server.RegisterEventHandler("curiosity:Server:Missions:CompletedMission", new Action<CitizenFX.Core.Player, bool>(OnCompletedMission));
        }

        static void OnCompletedMission([FromSource]CitizenFX.Core.Player player, bool passed)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{player.Handle}"))
            {
                Log.Error($"OnCompletedMission: Player session missing.");
                return;
            }

            if (passed)
            {
                // MONEY
                // Show success screen

            }
            else
            {
                // LOSE MONEY
                // Show success screen with fail
            }
        }

        static void OnKilledPed([FromSource]CitizenFX.Core.Player player, string data)
        {

            SkillMessage skillMessage = JsonConvert.DeserializeObject<SkillMessage>(Encode.BytesToStringConverted(Convert.FromBase64String(data)));

            if (!SessionManager.PlayerList.ContainsKey(skillMessage.PlayerHandle))
            {
                Log.Error($"OnKilledPed: Player session missing.");
                return;
            }

            if (!skillMessage.MissionPed)
            {
                Skills.DecreaseSkill(skillMessage.PlayerHandle, "policerep", 2);
                return;
            }

            if (skillMessage.Increase)
            {
                Skills.IncreaseSkill(skillMessage.PlayerHandle, skillMessage.Skill, random.Next(8, 10));
                Skills.IncreaseSkill(skillMessage.PlayerHandle, "knowledge", random.Next(3, 6));
                Skills.IncreaseSkill(skillMessage.PlayerHandle, "policerep", 1);
            }
            else
            {
                Skills.DecreaseSkill(skillMessage.PlayerHandle, skillMessage.Skill, 5);
                Skills.DecreaseSkill(skillMessage.PlayerHandle, "knowledge", 3);
                Skills.DecreaseSkill(skillMessage.PlayerHandle, "policerep", 1);
            }
        }
    }
}
