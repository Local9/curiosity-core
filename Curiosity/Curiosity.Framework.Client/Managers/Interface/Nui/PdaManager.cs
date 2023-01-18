using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Framework.Client.Managers.Interface.Nui
{
    public class PdaManager : Manager<PdaManager>
    {
        public override void Begin()
        {
            NuiManager.SetFocus(false, false);
        }

        [TickHandler]
        private async Task OnPdaControlManagerAsync()
        {
            if (Game.IsControlJustPressed(0, Control.FrontendSocialClub))
            {
                NuiManager.SendMessage(new { setVisible = true });
                NuiManager.SetFocus(true);
            }
        }
    }
}
