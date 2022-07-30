using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Police;
using Newtonsoft.Json;

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

                int ticketId = -1;
                int.TryParse(metadata.Find<string>(0), out ticketId);

                if (ticketId == -1)
                {
                    exportMessage.error = "Invalid Ticket ID";
                    return $"{exportMessage}";
                }

                exportMessage = await EventSystem.Request<ExportMessage>("police:suspect:ticket:pay", ticketId);

                Logger.Debug($"{exportMessage}");

                return $"{exportMessage}";
            }));

            Instance.AttachNuiHandler("PayOverdueTickets", new AsyncEventCallback(async metadata =>
            {
                ExportMessage exportMessage = new ExportMessage();

                exportMessage = await EventSystem.Request<ExportMessage>("police:suspect:ticket:pay:overdue");

                Notify.SendNui(eNotification.NOTIFICATION_INFO, "<b>Please wait, processing tickets</b>. You do not need to click the button again, please wait and you'll be notified when it is completed.");

                Logger.Debug($"{exportMessage}");

                return $"{exportMessage}";
            }));
        }
    }
}
