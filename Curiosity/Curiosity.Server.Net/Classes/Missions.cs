using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Server.net.Helpers;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Newtonsoft.Json;
using Curiosity.Server.net.Helpers;

namespace Curiosity.Server.net.Classes
{
    class Missions
    {
        static Server server = Server.GetInstance();
        static Random random = new Random();

        static Dictionary<string, int> activeMissions = new Dictionary<string, int>();

        static Dictionary<string, DateTime> timestampLastTrafficStop = new Dictionary<string, DateTime>();
        static Dictionary<string, DateTime> timestampLastArrest = new Dictionary<string, DateTime>();

        // Special Events
        static bool IsEventActive;
        static int PlayerHandleEventRegistered;

        public static void Init()
        {
            // Player who triggered it
            // Player to update
            // Is mission ped
            // increase or decrease

            API.RegisterCommand("mission", new Action<int, List<object>, string>(SendMission), false);

            server.RegisterEventHandler("curiosity:Server:Missions:TrafficStop", new Action<CitizenFX.Core.Player, string>(OnTrafficStop));
            server.RegisterEventHandler("curiosity:Server:Missions:ArrestedPed", new Action<CitizenFX.Core.Player, string>(OnArrestedPed));

            server.RegisterEventHandler("curiosity:Server:Missions:KilledPed", new Action<CitizenFX.Core.Player, string>(OnKilledPed));
            server.RegisterEventHandler("curiosity:Server:Missions:CompletedMission", new Action<CitizenFX.Core.Player, bool>(OnCompletedMission));
            server.RegisterEventHandler("curiosity:Server:Missions:StartedMission", new Action<CitizenFX.Core.Player, int>(OnStartedMission));
            server.RegisterEventHandler("curiosity:Server:Missions:EndMission", new Action<CitizenFX.Core.Player>(OnEndMission));


        }

        static void OnArrestedPed([FromSource]CitizenFX.Core.Player player, string encodedData)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;
            Session session = SessionManager.PlayerList[player.Handle];

            ArrestedPedData arrestedPed = JsonConvert.DeserializeObject<ArrestedPedData>(Encode.Base64ToString(encodedData));

            if (timestampLastArrest.ContainsKey(player.Handle))
            {
                DateTime dateTimeOfLastTrafficStop = timestampLastArrest[player.Handle];
                double secondsSinceLastArrest = (DateTime.Now - dateTimeOfLastTrafficStop).TotalSeconds;
                if (secondsSinceLastArrest < 40)
                {
                    return;
                }
            }
            else
            {
                timestampLastArrest.Add(player.Handle, DateTime.Now);
            }

            float experienceMultiplier = 1.0f; // Base value
            float moneyMultiplier = 1.0f; // Base value

            if (arrestedPed.IsCarryingIllegalItems)
            {
                experienceMultiplier += .5f;
                moneyMultiplier += .5f;
            }

            if (arrestedPed.IsDrugged || arrestedPed.IsDrunk)
            {
                experienceMultiplier += 1f;
                moneyMultiplier += 1f;
            }

            if (arrestedPed.IsDrivingStolenCar)
            {
                experienceMultiplier += 4f;
                moneyMultiplier += 4f;
            }

            if (arrestedPed.CaughtSpeeding)
            {
                experienceMultiplier += 1.5f;
                moneyMultiplier += 1.5f;
            }

            if (!arrestedPed.IsAllowedToBeArrested)
            {
                experienceMultiplier = 0.1f;
                moneyMultiplier = 0.1f;

                Skills.DecreaseSkill(player.Handle, "policerep", 2);
            }
            else
            {
                Skills.IncreaseSkill(player.Handle, "policerep", 1);
            }

            int exp = random.Next(10, 30);
            int knowledge = random.Next(6, 10);
            int money = random.Next(60, 100);

            int experienceEarnAdditional = (int)(exp * experienceMultiplier);
            int knowledgeEarnAdditional = (int)(knowledge * experienceMultiplier);
            int moneyEarnAdditional = (int)(money * moneyMultiplier);

            Skills.IncreaseSkill(player.Handle, "policexp", experienceEarnAdditional);
            Skills.IncreaseSkill(player.Handle, "knowledge", knowledgeEarnAdditional);
            Bank.IncreaseCashInternally(player.Handle, moneyEarnAdditional);
            timestampLastArrest[player.Handle] = DateTime.Now;
        }

        static void OnTrafficStop([FromSource]CitizenFX.Core.Player player, string encodedData)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;
            Session session = SessionManager.PlayerList[player.Handle];

            if (timestampLastTrafficStop.ContainsKey(player.Handle))
            {
                DateTime dateTimeOfLastTrafficStop = timestampLastTrafficStop[player.Handle];
                double secondsSinceLastTrafficStop = (DateTime.Now - dateTimeOfLastTrafficStop).TotalSeconds;
                if (secondsSinceLastTrafficStop < 59)
                {
                    session.IsCheater = true;
                    session.Player.TriggerEvent("curiosity:Client:Player:UpdateExtraFlags");
                    return;
                }
            }

            if (string.IsNullOrEmpty(encodedData))
            {
                Skills.IncreaseSkill(player.Handle, "policexp", random.Next(1, 6));
                Skills.IncreaseSkill(player.Handle, "knowledge", random.Next(1, 4));
                Skills.IncreaseSkill(player.Handle, "policerep", 1);
                Bank.IncreaseCashInternally(player.Handle, 15);
            }
            else
            {
                TrafficStopData trafficStopData = JsonConvert.DeserializeObject<TrafficStopData>(Encode.Base64ToString(encodedData));

                if (trafficStopData.Ticket)
                {
                    Skills.IncreaseSkill(player.Handle, "policexp", random.Next(6, 12));
                    Skills.IncreaseSkill(player.Handle, "knowledge", random.Next(3, 6));
                    Skills.IncreaseSkill(player.Handle, "policerep", 1);
                    Bank.IncreaseCashInternally(player.Handle, 25);
                }
            }
            timestampLastTrafficStop[player.Handle] = DateTime.Now;
        }

        static void SendMission(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            if (arguments.Count < 2)
            {
                Helpers.Notifications.Advanced($"Agruments Missing", $"", 2, session.Player);
                return;
            }

            MissionCreate missionCreate = new MissionCreate()
            {
                MissionId = int.Parse($"{arguments[0]}"),
                PatrolZone = int.Parse($"{arguments[1]}")
            };

            string json = JsonConvert.SerializeObject(missionCreate);

            string encoded = Encode.StringToBase64(json);

            session.Player.TriggerEvent("curiosity:Client:Mission:Start", encoded);
        }

        static void OnEndMission([FromSource]CitizenFX.Core.Player player)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{player.Handle}"))
            {
                Log.Error($"OnCompletedMission: Player session missing.");
                return;
            }

            Session session = SessionManager.PlayerList[player.Handle];

            if (activeMissions.ContainsKey(session.License))
            {
                activeMissions.Remove(session.License);
            }
        }

        static void OnStartedMission([FromSource]CitizenFX.Core.Player player, int missionId)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{player.Handle}"))
            {
                Log.Error($"OnCompletedMission: Player session missing.");
                return;
            }

            Session session = SessionManager.PlayerList[player.Handle];

            if (activeMissions.ContainsKey(session.License))
            {
                activeMissions.Remove(session.License);
            }

            activeMissions.Add(session.License, missionId);
        }

        static void OnCompletedMission([FromSource]CitizenFX.Core.Player player, bool passed)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle))
            {
                Log.Error($"OnCompletedMission: Player session missing.");
                return;
            }

            // Check player has active mission
            Session session = SessionManager.PlayerList[player.Handle];

            string title = passed ? "Completed" : "Failed";
            MissionMessage missionMessage = new MissionMessage($"Mission {title}");

            //if (!activeMissions.ContainsKey(session.License) && !session.IsDeveloper)
            //{
            //    session.IsCheater = true;
            //    session.Player.TriggerEvent("curiosity:Client:Player:UpdateFlags");
            //    return;
            //}

            if (passed)
            {
                missionMessage.MissionCompleted = 1;
                missionMessage.MoneyEarnt = 100;
                missionMessage.HostagesRescued = 1;
                Bank.IncreaseCashInternally(player.Handle, missionMessage.MoneyEarnt);
                Skills.IncreaseSkill(player.Handle, "policexp", 15);
            }
            else
            {
                missionMessage.MissionCompleted = 0;
                missionMessage.HostagesRescued = 0;
                //missionMessage.MoneyLost = 100;
                //Bank.DecreaseCashInternally(player.Handle, missionMessage.MoneyLost);
            }

            string subTitle = passed ? "Successful" : "Unsuccessful";

            session.Player.Send(NotificationType.CHAR_CALL911, 2, "Dispatch Complete", subTitle, $"Hostages Saved: ~y~{missionMessage.HostagesRescued}");

            session.Player.TriggerEvent("curiosity:Client:Missions:MissionComplete");
            
            ChatLog.SendLogMessage($"Mission Completed: {subTitle}", session.Player);
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
                Skills.IncreaseSkill(skillMessage.PlayerHandle, "policexp", random.Next(8, 10));
                Skills.IncreaseSkill(skillMessage.PlayerHandle, "knowledge", random.Next(3, 6));
                Skills.IncreaseSkill(skillMessage.PlayerHandle, "policerep", 1);
            }
            else
            {
                Skills.DecreaseSkill(skillMessage.PlayerHandle, "policexp", 5);
                Skills.DecreaseSkill(skillMessage.PlayerHandle, "knowledge", 3);
                Skills.DecreaseSkill(skillMessage.PlayerHandle, "policerep", 1);
            }
        }
    }
}
