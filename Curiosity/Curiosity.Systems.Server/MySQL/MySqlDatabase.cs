using MySql.Data.MySqlClient;
using System;

namespace Curiosity.Systems.Server.MySQL
{
    public class MySqlDatabase : IDisposable
    {
        public readonly MySqlConnection Connection;

        public MySqlDatabase()
        {
            Connection = new MySqlConnection(CuriosityPlugin.DatabaseConnectionString);
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
