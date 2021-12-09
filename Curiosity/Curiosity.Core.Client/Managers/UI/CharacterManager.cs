using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Police;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Managers.UI
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public override void Begin()
        {
            Instance.AttachNuiHandler("GetTickets", new AsyncEventCallback(async metadata =>
            {
                List<PoliceTicket> policeTickets = await EventSystem.Request<List<PoliceTicket>>("police:suspect:ticket:get");
                return JsonConvert.SerializeObject(policeTickets);
            }));

            Instance.AttachNuiHandler("PayTicket", new AsyncEventCallback(async metadata =>
            {
                ExportMessage exportMessage = new ExportMessage();

                if (!int.TryParse(metadata.Find<string>(0), out int ticketId))
                {
                    exportMessage.error = "Invalid Ticket ID";
                    return $"{exportMessage}";
                }

                exportMessage = await EventSystem.Request<ExportMessage>("police:suspect:ticket:pay", ticketId);

                Logger.Debug($"{exportMessage}");

                return $"{exportMessage}";
            }));
        }
    }
}
