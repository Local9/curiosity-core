using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Curiosity.Server.net.Database
{
    public class DatabaseBank
    {
        MySQL mySql;

        static DatabaseBank _bank;
        public static DatabaseBank ActiveInstance = _bank;

        public static DatabaseBank GetInstance()
        {
            return _bank;
        }

        public DatabaseBank()
        {
            mySql = DatabaseSettings.GetInstance().mySQL;
            _bank = this;
        }
    }
}
