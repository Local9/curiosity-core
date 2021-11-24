using CitizenFX.Core;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models.Police;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    internal class PoliceDatabase
    {
        public static async Task<bool> InsertTicket(ePoliceTicketType policeTicketType, int characterId, int characterVehicleId, long ticketValue, DateTime paymentDue, int vehicleSpeed, int speedLimit)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@pPoliceTicketTypeId", (int)policeTicketType },
                    { "@pCharacterId", characterId },
                    { "@pCharacterVehicleId", characterVehicleId },
                    { "@pTicketValue", ticketValue },
                    { "@pTicketPaymentDue", paymentDue },
                    { "@pVehicleSpeed", paymentDue },
                    { "@pSpeedLimit", paymentDue },
                };

            string myQuery = "call insCharacterTicket(@pPoliceTicketTypeId, @pCharacterId, @pCharacterVehicleId, @pTicketValue, @pTicketPaymentDue, @pVehicleSpeed, @pSpeedLimit);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    throw new Exception("Unable to insert ticket");

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    return kv["Result"].ToBoolean();
                }
            }

            return false;
        }

        public static async Task<List<PoliceTicket>> GetTickets(int characterId)
        {
            List<PoliceTicket> policeTickets = new List<PoliceTicket>();

            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@pCharacterId", characterId },
                };

            string myQuery = "call selCharacterTickets(@pCharacterId);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    throw new Exception("Unable to insert ticket");

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    PoliceTicket pt = new();
                }
            }


            return policeTickets;
        }
    }
}
