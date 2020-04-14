using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Managers
{
    public class CoreManager : Manager<CoreManager>
    {
        public override void Begin()
        {
            
        }

        [TickHandler(SessionWait = true)]
        public async Task OnCoreControls()
        {
            if (Game.IsControlJustPressed(0, Control.FrontendSocialClubSecondary))
            {
                Notify.Alert("Core Menu Open");
            }
        }
    }
}
