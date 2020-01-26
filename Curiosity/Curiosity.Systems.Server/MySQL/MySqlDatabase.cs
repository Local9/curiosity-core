using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
