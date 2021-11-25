using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models.Police;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Instance.AttachNuiHandler("PayTicket", new AsyncEventCallback(async metadata =>
            {
                return null;
            }));
        }
    }
}
