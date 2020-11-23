using CitizenFX.Core;
using Curiosity.Server.net.Classes;
using Curiosity.Server.net.Database;
using Curiosity.Server.net.Entity;

namespace Curiosity.Server.net.Business
{
    public class Mission : BaseScript
    {
        public Mission()
        {

        }

        public async static void RecordMissionCompletion(string playerSource, string missionId, bool passed)
        {
            if (!SessionManager.PlayerList.ContainsKey(playerSource)) return;

            Session session = SessionManager.PlayerList[playerSource];

            MissionData missionData = await DatabaseMission.GetMissionAsync(missionId);

            int xpReward = missionData.XpReward;
            int repReward = missionData.RepReward;
            int repFailure = missionData.RepFailure;
            int cashMin = missionData.CashMin;
            int cashMax = missionData.CashMax;

            if (passed)
            {
                int money = Server.random.Next(cashMin, cashMax);

                await BaseScript.Delay(100);
                Skills.IncreaseSkillByPlayerExport(playerSource, "knowledge", 2);

                DatabaseUsersBank.IncreaseCash(session.User.BankId, money);
                session.IncreaseWallet(money);
                session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);

                await BaseScript.Delay(100);
                Skills.IncreaseSkillByPlayerExport(playerSource, "police", xpReward);

                await BaseScript.Delay(100);
                Skills.IncreaseSkillByPlayerExport(playerSource, "policerep", repReward);
            }
            else
            {
                await BaseScript.Delay(100);
                Skills.DecreaseSkillByPlayerExport(playerSource, "knowledge", 4);
                await BaseScript.Delay(100);
                Skills.DecreaseSkillByPlayerExport(playerSource, "policerep", repFailure);
            }
        }
    }
}
