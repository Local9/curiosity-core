using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Server.net.Entity;
using Curiosity.Server.net.Helpers;
using Curiosity.Shared.Server.net.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

            server.RegisterEventHandler("curiosity:server:callout:completed", new Action<CitizenFX.Core.Player, string>(OnCalloutCompleted));

            server.RegisterEventHandler("curiosity:Server:Missions:TrafficStop", new Action<CitizenFX.Core.Player, string>(OnTrafficStop));
            server.RegisterEventHandler("curiosity:Server:Missions:ArrestedPed", new Action<CitizenFX.Core.Player, string>(OnArrestedPed));

            server.RegisterEventHandler("curiosity:Server:Missions:KilledPed", new Action<CitizenFX.Core.Player, string>(OnKilledPed));
            server.RegisterEventHandler("curiosity:Server:Missions:CompletedMission", new Action<CitizenFX.Core.Player, bool>(OnCompletedMission));
            server.RegisterEventHandler("curiosity:Server:Missions:StartedMission", new Action<CitizenFX.Core.Player, int>(OnStartedMission));
            server.RegisterEventHandler("curiosity:Server:Missions:EndMission", new Action<CitizenFX.Core.Player>(OnEndMission));
            server.RegisterEventHandler("curiosity:Server:Missions:VehicleTowed", new Action<CitizenFX.Core.Player>(OnVehicleTowed));
        }

        static void OnCalloutCompleted([FromSource] CitizenFX.Core.Player player, string encodedData)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;
            Session session = SessionManager.PlayerList[player.Handle];

            CalloutMessage calloutMessage = JsonConvert.DeserializeObject<CalloutMessage>(Encode.Base64ToString(encodedData));

            string subTitle = "Maybe next time...";
            if (calloutMessage.Success)
                subTitle = "~g~Completed";

            int experience = 50;
            int knowledge = 10;
            int money = 100;

            string content = string.Empty;
            switch(calloutMessage.CalloutType)
            {
                case CalloutType.HOSTAGE_RESCUE:
                    content = $"~b~Hostages Rescued~s~: {calloutMessage.NumberRescued}";

                    experience = Server.random.Next(50, 80) * calloutMessage.NumberRescued;
                    knowledge = Server.random.Next(5, 10) * calloutMessage.NumberRescued;
                    money = Server.random.Next(60, 120) * calloutMessage.NumberRescued;

                    break;
            }

            if (Server.IsBirthday)
            {
                experience = (int)(experience * 2.0f);
                money = (int)(money * 2.0f);
            }

            // Experience, skills, and money... need to add some event fun
            Skills.IncreaseSkill(player.Handle, "policexp", experience);
            Skills.IncreaseSkill(player.Handle, "knowledge", knowledge);
            Bank.IncreaseCashInternally(player.Handle, money);

            session.Player.Send(NotificationType.CHAR_CALL911, 2, "Callout Completed", subTitle, content);

            // MessagePolicePlayers(session.Player, "Dispatch", string.Empty, $"{session.Player.Name} completed callout");
        }

        private static void OnVehicleTowed([FromSource] CitizenFX.Core.Player player)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;
            Session session = SessionManager.PlayerList[player.Handle];
            Bank.IncreaseCashInternally(player.Handle, 50);
            Server.TriggerEvent("elv:police:towed");
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

            List<string> wrapSheet = new List<string>();
            float experienceMultiplier = 1.0f; // Base value
            float moneyMultiplier = 1.0f; // Base value

            if (arrestedPed.IsCarryingIllegalItems)
            {
                experienceMultiplier += .5f;
                moneyMultiplier += .5f;
                wrapSheet.Add("Illegal Items");
            }

            if (arrestedPed.IsWanted)
            {
                experienceMultiplier += 1f;
                moneyMultiplier += 1f;
                wrapSheet.Add("Wanted");
            }

            if (arrestedPed.IsDrugged || arrestedPed.IsDrunk)
            {
                experienceMultiplier += 1f;
                moneyMultiplier += 1f;
                wrapSheet.Add("Under the Influence");
            }

            if (arrestedPed.IsDrivingStolenCar)
            {
                experienceMultiplier += 4f;
                moneyMultiplier += 4f;
                wrapSheet.Add("Stolen Car");
            }

            if (arrestedPed.CaughtSpeeding)
            {
                experienceMultiplier += 1.5f;
                moneyMultiplier += 1.5f;
                wrapSheet.Add("Caught Speeding");
            }

            if (Server.IsBirthday)
            {
                experienceMultiplier += 2.0f;
                moneyMultiplier += 2.0f;
            }

            if (!arrestedPed.IsAllowedToBeArrested)
            {
                experienceMultiplier = 0.1f;
                moneyMultiplier = 0.1f;

                wrapSheet.Add("Was found Innocent");

                Skills.DecreaseSkill(player.Handle, "policerep", 2);
            }
            else
            {
                BaseScript.TriggerEvent("elv:community:arrest");
                Skills.IncreaseSkill(player.Handle, "policerep", 1);
            }

            int exp = random.Next(20, 50);
            int knowledge = random.Next(6, 15);
            int money = random.Next(60, 125);

            int experienceEarnAdditional = (int)(exp * experienceMultiplier);
            int knowledgeEarnAdditional = (int)(knowledge * experienceMultiplier);
            int moneyEarnAdditional = (int)(money * moneyMultiplier);

            if (arrestedPed.DispatchJail)
            {
                float lostPct = arrestedPed.IsBike ? .75f : .5f;

                experienceEarnAdditional = (int)(experienceEarnAdditional * lostPct);
                knowledgeEarnAdditional = (int)(knowledgeEarnAdditional * lostPct);
                moneyEarnAdditional = (int)(moneyEarnAdditional * lostPct);
            }

            if (experienceEarnAdditional >= 1000)
            {
                experienceEarnAdditional = 900;
            }

            if (knowledgeEarnAdditional >= 1000)
            {
                knowledgeEarnAdditional = 900;
            }

            if (knowledgeEarnAdditional == 0)
            {
                knowledgeEarnAdditional = random.Next(3, 6);
            }

            Skills.IncreaseSkill(player.Handle, "policexp", experienceEarnAdditional);
            Skills.IncreaseSkill(player.Handle, "knowledge", knowledgeEarnAdditional);
            Bank.IncreaseCashInternally(player.Handle, moneyEarnAdditional);
            timestampLastArrest[player.Handle] = DateTime.Now;

            string subTitle = "~g~Reward: 100%";
            if (arrestedPed.DispatchJail)
            {
                subTitle = "~y~Reward: 50%";
            }

            if (arrestedPed.IsBike)
            {
                subTitle = "~y~Reward: 75%";
            }

            session.Player.Send(NotificationType.CHAR_CALL911, 2, "Suspect Booked", subTitle, $"Experience: ~b~{experienceEarnAdditional:N} XP~n~~s~Knowledge: ~b~{knowledgeEarnAdditional:N}~n~~s~Payout: ~b~${moneyEarnAdditional:C}");
            session.Player.Send(NotificationType.CHAR_CALL911, 2, "Suspect Booked", "Wrap Sheet", string.Join(", ", wrapSheet));

            MessagePolicePlayers(session.Player, "Dispatch", string.Empty, $"{session.Player.Name} has arrested a suspect");
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

            int experience = random.Next(1, 6);
            int knowledge = random.Next(1, 4);

            if (Server.IsBirthday)
            {
                experience = (int)(experience * 2.0f);
                knowledge = (int)(knowledge * 2.0f);
            }

            if (string.IsNullOrEmpty(encodedData))
            {
                Skills.IncreaseSkill(player.Handle, "policexp", experience);
                Skills.IncreaseSkill(player.Handle, "knowledge", knowledge);
                Skills.IncreaseSkill(player.Handle, "policerep", 1);
                Bank.IncreaseCashInternally(player.Handle, 15);

                MessagePolicePlayers(session.Player, "Dispatch", string.Empty, $"{session.Player.Name} has completed a traffic stop");
            }
            else
            {
                TrafficStopData trafficStopData = JsonConvert.DeserializeObject<TrafficStopData>(Encode.Base64ToString(encodedData));

                if (trafficStopData.Ticket)
                {

                    experience = random.Next(6, 16);
                    knowledge = random.Next(3, 6);

                    if (Server.IsBirthday)
                    {
                        experience = (int)(experience * 2.0f);
                        knowledge = (int)(knowledge * 2.0f);
                    }

                    Skills.IncreaseSkill(player.Handle, "policexp", experience);
                    Skills.IncreaseSkill(player.Handle, "knowledge", knowledge);
                    Skills.IncreaseSkill(player.Handle, "policerep", 1);
                    Bank.IncreaseCashInternally(player.Handle, 25);

                    MessagePolicePlayers(session.Player, "Dispatch", string.Empty, $"{session.Player.Name} has issued a speeding ticket");
                }
            }
            timestampLastTrafficStop[player.Handle] = DateTime.Now;
        }

        static void MessagePolicePlayers(CitizenFX.Core.Player player, string title, string subtitle, string message)
        {
            Dictionary<string, Session> police = SessionManager.PlayerList.Select(x => x).Where(x => x.Value.JobMessages && x.Value.job == Job.Police).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<string, Session> valuePair in police)
            {
                Session session = valuePair.Value;

                if (player.Handle == session.Player.Handle) continue;
                
                if (session.job == Job.Police && session.JobMessages)
                {
                    session.Player.Send(NotificationType.CHAR_CALL911, 2, title, subtitle, message);
                }
            }
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

            float multiplier = (Server.IsBirthday) ? 3.0f : 1.0f;

            string message = "has failed to rescue the hostage";

            if (passed)
            {
                message = "has rescued the hostage";
                missionMessage.MissionCompleted = 1;
                missionMessage.MoneyEarnt = (int)(100 * multiplier);
                missionMessage.HostagesRescued = 1;
                Bank.IncreaseCashInternally(player.Handle, missionMessage.MoneyEarnt);
                Skills.IncreaseSkill(player.Handle, "policexp", (int)(15 * multiplier));
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

            MessagePolicePlayers(session.Player, "Dispatch", string.Empty, $"{session.Player.Name} {message}");
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
                int experience = random.Next(8, 10);
                int knowledge = random.Next(3, 6);

                if (skillMessage.IsHeadshot)
                {
                    experience = experience * 2;
                    knowledge = knowledge * 2;
                }

                if (Server.IsBirthday)
                {
                    experience = experience * 2;
                    knowledge = knowledge * 2;
                }

                Skills.IncreaseSkill(skillMessage.PlayerHandle, "policexp", experience);
                Skills.IncreaseSkill(skillMessage.PlayerHandle, "knowledge", knowledge);
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
