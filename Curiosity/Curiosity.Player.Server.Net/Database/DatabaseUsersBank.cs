using CitizenFX.Core;
using GHMatti.Data.MySQL;
using System.Collections.Generic;

namespace Curiosity.Server.net.Database
{
    public class DatabaseUsersBank : BaseScript
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static void IncreaseCash(long characterBankId, int amount)
        {
            string query = "UPDATE character_bank set `wallet` = `wallet` + @wallet where characterBankId = @bankId;";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterBankId", characterBankId);
            myParams.Add("@wallet", amount);
            mySql.Query(query, myParams);
        }

        public static void DecreaseCash(long characterBankId, int amount)
        {
            string query = "UPDATE character_bank set `wallet` = `wallet` - @wallet where characterBankId = @bankId;";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterBankId", characterBankId);
            myParams.Add("@wallet", amount);
            mySql.Query(query, myParams);
        }

        public static void IncreaseBank(long characterBankId, int amount)
        {
            string query = "UPDATE character_bank set `bankAccount` = `bankAccount` + @bankAccount where characterBankId = @bankId;";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterBankId", characterBankId);
            myParams.Add("@bankAccount", amount);
            mySql.Query(query, myParams);
        }

        public static void DecreaseBank(long characterBankId, int amount)
        {
            string query = "UPDATE character_bank set `bankAccount` = `bankAccount` - @bankAccount where characterBankId = @bankId;";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterBankId", characterBankId);
            myParams.Add("@bankAccount", amount);
            mySql.Query(query, myParams);
        }
    }
}
