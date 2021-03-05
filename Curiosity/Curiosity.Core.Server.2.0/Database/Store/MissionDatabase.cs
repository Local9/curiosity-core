using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class MissionDatabase
    {
        public static async Task<Mission> Get(string missionKey)
        {
            Mission missionData = new Mission();

            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@missionId", missionKey }
                };

            string myQuery = "call selMissionInformation(@missionId);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    return null;

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    missionData.MissionId = $"{kv["id"]}";
                    missionData.XpReward = kv["xpReward"].ToInt();
                    missionData.RepReward = kv["repReward"].ToInt();
                    missionData.RepFailure = kv["repFailure"].ToInt();
                    missionData.CashMin = kv["cashMin"].ToInt();
                    missionData.CashMax = kv["cashMax"].ToInt();
                }
            }

            return missionData;
        }

        public static async Task<bool> Passed(int characterId, int numberOfArrests)
        {
            throw new NotImplementedException();
        }
        public static async Task<bool> Failed(int characterId, string missionKey)
        {
            throw new NotImplementedException();
        }
    }
}
