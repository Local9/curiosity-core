using MySql.Data.MySqlClient;
using System;

namespace Curiosity.Systems.Server.MySQL
{
    public class MySqlDatabase : IDisposable
    {
        public readonly MySqlConnection Connection;

        public MySqlDatabase()
        {
            string connectionStr = $"{CuriosityPlugin.DatabaseConnectionString};Min Pool Size=10;";
            Connection = new MySqlConnection(connectionStr);
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
