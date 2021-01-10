using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Core.Server.Extensions;
using GHMatti.Data.MySQL.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class BankDatabase
    {
        // process
        // buy item -> tell server item user wants to buy
        // server adds item to players inventory/account depending on type
        // on completion, call back to the client and inform purchase

        public static async Task DecreaseCash(ulong discordId, int amount)
        {

        }

        public static async Task IncreaseCash(ulong discordId, int amount)
        {

        }
    }
}
