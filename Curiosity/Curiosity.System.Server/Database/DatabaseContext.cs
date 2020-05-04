using System.Data.Entity;
using CitizenFX.Core.Native;
using MySql.Data.Entity;

namespace Curiosity.System.Server.Database
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class DatabaseContext<TContext> : DbContext where TContext : DbContext
    {
        static DatabaseContext()
        {
            Database.SetInitializer<TContext>(null);
        }

        protected DatabaseContext() : base(API.GetConvar("mysql",
            "server=127.0.0.1;port=3306;database=curiosity;username=root;password=;"))
        {
        }
    }
}