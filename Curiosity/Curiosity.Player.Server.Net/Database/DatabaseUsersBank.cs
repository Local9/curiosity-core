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

        public static void IncreaseCash(long userId, int cash)
        {
            string query = "INSERT INTO usersbank (`userId`,`cash`,`serverId`)" +
                " VALUES (@userId, @cash, @serverId)" +
                " ON DUPLICATE KEY UPDATE `cash` = `cash` + @cash;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@cash", cash);
            myParams.Add("@serverId", Server.serverId);

            Debug.WriteLine("mysql");

            mySql.Query(query, myParams);
        }

        public static void DecreaseCash(long userId, int cash)
        {
            string query = "INSERT INTO usersbank (`userId`,`cash`,`serverId`)" +
                " VALUES (@userId, @cash, @serverId)" +
                " ON DUPLICATE KEY UPDATE `cash` = `cash` - @cash;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@cash", cash);
            myParams.Add("@serverId", Server.serverId);

            mySql.Query(query, myParams);
        }

        public static void IncreaseBank(long userId, int bank)
        {
            string query = "INSERT INTO usersbank (`userId`,`bank`,`serverId`)" +
                " VALUES (@userId, @bank, @serverId)" +
                " ON DUPLICATE KEY UPDATE `bank` = `bank` + @bank;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@bank", bank);
            myParams.Add("@serverId", Server.serverId);

            mySql.Query(query, myParams);
        }

        public static void DecreaseBank(long userId, int bank)
        {
            string query = "INSERT INTO usersbank (`userId`,`bank`,`serverId`)" +
                 " VALUES (@userId, @bank, @serverId)" +
                 " ON DUPLICATE KEY UPDATE `bank` = `bank` - @bank;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@bank", bank);
            myParams.Add("@serverId", Server.serverId);

            mySql.Query(query, myParams);
        }
    }
}
