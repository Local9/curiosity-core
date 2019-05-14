using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Curiosity.Server.net.Classes
{
    public class Bank : BaseScript
    {
        Database.DatabaseBank databaseBank = Database.DatabaseBank.GetInstance();

        public Bank()
        {
            //Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:IncreaseCash", new Action<Player>(IncreaseCash));
            //Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:DecreaseCash", new Action<Player>(IncreaseCash));
            //Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:IncreaseBank", new Action<Player>(IncreaseCash));
            //Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:DecreaseBank", new Action<Player>(IncreaseCash));
        }
    }
}
