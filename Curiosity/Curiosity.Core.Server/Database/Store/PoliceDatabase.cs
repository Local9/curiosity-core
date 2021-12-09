using CitizenFX.Core;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models.Police;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
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
                    { "@pVehicleSpeed", vehicleSpeed },
                    { "@pSpeedLimit", speedLimit },
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

        public static async Task<List<PoliceTicket>> GetTickets(int characterId, bool unpaid = true)
        {
            List<PoliceTicket> policeTickets = new List<PoliceTicket>();

            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@pCharacterId", characterId },
                    { "@pUnpaid", unpaid },
                };

            string myQuery = "call selCharacterTickets(@pCharacterId, @pUnpaid);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    throw new Exception("Unable to insert ticket");

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    PoliceTicket pt = new();

                    pt.Id = kv["ID"].ToInt();
                    pt.PoliceTicketTypeId = (ePoliceTicketType)kv["PoliceTicketTypeId"].ToInt();
                    pt.CharacterId = kv["CharacterId"].ToInt();
                    pt.CharacterName = $"{kv["CharacterName"]}";
                    pt.TicketDate = kv["TicketDate"].ToDateTime();
                    pt.TicketPaymentDue = kv["TicketPaymentDue"].ToDateTime();
                    pt.PaymentOverdue = kv["PaymentOverdue"].ToBoolean();

                    if (kv.ContainsValue("TicketPaid"))
                        pt.TicketPaid = kv["TicketPaid"].ToDateTime();

                    pt.TicketValue = kv["TicketValue"].ToInt();
                    pt.VehicleSpeed = kv["VehicleSpeed"].ToInt();
                    pt.SpeedLimit = kv["SpeedLimit"].ToInt();

                    policeTickets.Add(pt);
                }
            }


            return policeTickets;
        }

        internal static async Task<bool> PayTicket(int characterId, int ticketId)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@pPoliceTicketId", ticketId },
                    { "@pCharacterId", characterId },
                };

            string myQuery = "call upCharacterTicketPaid(@pPoliceTicketId, @pCharacterId);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    throw new Exception("Unable to update ticket");

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    return kv["Result"].ToBoolean();
                }
            }

            return false;
        }
    }
}
