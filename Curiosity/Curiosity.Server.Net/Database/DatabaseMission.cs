using CitizenFX.Core;
using Curiosity.Server.net.Entity;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Database
{
    class DatabaseMission
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<MissionData> GetMissionAsync(string missionId)
        {
            MissionData missionData = new MissionData();

            string query = "call selMissionInformation(missionId);";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@missionId", missionId);

            using (var result = mySql.QueryResult(query, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    return null;
                }

                foreach (Dictionary<string, object> keyValues in keyValuePairs)
                {
                    missionData.MissionId = $"{keyValues["id"]}";
                    missionData.XpReward = int.Parse($"{keyValues["xpReqard"]}");
                    missionData.RepReward = int.Parse($"{keyValues["repReward"]}");
                    missionData.RepFailure = int.Parse($"{keyValues["repFailure"]}");
                    missionData.CashMin = int.Parse($"{keyValues["cashMin"]}");
                    missionData.CashMax = int.Parse($"{keyValues["cashMax"]}");
                };

                return missionData;
            }
        }
    }
}
