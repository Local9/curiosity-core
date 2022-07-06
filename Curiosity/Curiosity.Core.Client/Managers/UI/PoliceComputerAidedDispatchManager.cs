using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models.Police;
using Newtonsoft.Json;

namespace Curiosity.Core.Client.Managers.UI
{
    public class PoliceComputerAidedDispatchManager : Manager<PoliceComputerAidedDispatchManager>
    {
        public override void Begin()
        {
            Instance.AttachNuiHandler("GetCharacterTickets", new AsyncEventCallback(async metadata =>
            {
                List<PoliceTicket> policeTicketList = await EventSystem.Request<List<PoliceTicket>>("police:get:suspect:tickets");
                return JsonConvert.SerializeObject(policeTicketList);
            }));
        }
    }
}
