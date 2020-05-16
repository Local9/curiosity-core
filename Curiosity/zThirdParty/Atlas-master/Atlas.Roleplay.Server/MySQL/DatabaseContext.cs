using CitizenFX.Core.Native;
using MySql.Data.Entity;
using System.Data.Entity;

namespace Atlas.Roleplay.Server.MySQL
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class DatabaseContext<TContext> : DbContext where TContext : DbContext
    {
        static DatabaseContext()
        {
            Database.SetInitializer<TContext>(null);
        }

        protected DatabaseContext() : base(API.GetConvar("mysql",
            "server=127.0.0.1;port=3306;database=roleplay;username=root;password=;"))
        {
        }
    }
}