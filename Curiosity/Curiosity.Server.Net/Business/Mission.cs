using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Server.net.Classes;
using Curiosity.Server.net.Database;
using Curiosity.Server.net.Entity;
using Curiosity.Server.net.Helpers;
using Curiosity.Shared.Server.net.Helpers;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Business
{
    public class Mission : BaseScript
    {
        public Mission()
        {

        }

        public async static Task<MissionData> RecordMissionCompletion(string playerSource, string missionId, bool passed, int numTransportArrested, int numberOfFailures)
        {
            if (!SessionManager.PlayerList.ContainsKey(playerSource)) return null;

            Session session = SessionManager.PlayerList[playerSource];

            MissionData missionData = await DatabaseMission.GetMissionAsync(missionId);

            if (missionData == null)
            {
                Log.Error($"No mission returned from the database matching the ID {missionId} [{playerSource}|{missionId}|{passed}]");
                return null;
            }

            bool usedTransport = numTransportArrested > 0;

            int xpReward = missionData.XpReward;
            int repReward = missionData.RepReward;
            int repFailure = missionData.RepFailure;
            int cashMin = missionData.CashMin;
            int cashMax = missionData.CashMax;

            if (passed)
            {
                if (usedTransport)
                {
                    xpReward = (int)(xpReward * .5f);
                    repReward = (int)(repReward * .5f);
                    cashMin = (int)(cashMin * .5f);
                    cashMax = (int)(cashMax * .5f);
                }

                if (numberOfFailures >= 3)
                {
                    xpReward = (int)(xpReward * .1f);
                    repReward = 0;
                    cashMin = (int)(cashMin * .1f);
                    cashMax = (int)(cashMax * .1f);
                }

                int money = Server.random.Next(cashMin, cashMax);

                await BaseScript.Delay(100);
                Skills.IncreaseSkillByPlayerExport(playerSource, "knowledge", 2);

                DatabaseUsersBank.IncreaseCash(session.User.BankId, money);
                session.IncreaseWallet(money);
                session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);

                await BaseScript.Delay(100);
                Skills.IncreaseSkillByPlayerExport(playerSource, "policexp", xpReward);

                await BaseScript.Delay(100);
                Skills.IncreaseSkillByPlayerExport(playerSource, "policerep", repReward);

                missionData.RepFailure = 0;

                float experienceModifier = float.Parse(API.GetConvar("experience_modifier", $"1.0"));

                if (experienceModifier > 1.0f && (session.IsStaff || session.IsDonator))
                {
                    experienceModifier += 0.1f;
                }

                if (session.IsStaff || session.IsDonator)
                {
                    experienceModifier += Skills.ExperienceModifier(session.Privilege);
                }

                xpReward = (int)(xpReward * experienceModifier);

                // send success notification
                session.Player.Send(NotificationType.CHAR_CALL911, 2, "Dispatch A.I.", "Completed", $"~b~XP Gained~w~: {xpReward:d0}~n~~b~Rep Gained~w~: {repReward:d0}~n~~b~Cash~w~: ${money:c0}");
            }
            else
            {
                missionData.XpReward = 0;
                missionData.RepReward = 0;
                missionData.CashMax = 0;
                missionData.CashMin = 0;

                await BaseScript.Delay(100);
                Skills.DecreaseSkillByPlayerExport(playerSource, "knowledge", 4);
                await BaseScript.Delay(100);
                Skills.DecreaseSkillByPlayerExport(playerSource, "policerep", repFailure);

                // send failure notification
                session.Player.Send(NotificationType.CHAR_CALL911, 2, "Dispatch A.I.", "Failed", $"~b~Rep Lost~w~: {repFailure:d0}");
            }

            return missionData;
        }
    }
}
