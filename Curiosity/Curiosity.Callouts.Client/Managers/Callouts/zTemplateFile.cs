﻿using CitizenFX.Core;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class Template : Callout
    {
        private CalloutMessage calloutMessage = new CalloutMessage();
        public Template(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        internal async override void Prepare()
        {
            base.Prepare();

            base.IsSetup = true;
        }
        internal override void End(bool forcefully = false, CalloutMessage cm = null)
        {
            cm = calloutMessage;
            base.End(forcefully);
        }

        internal async override void Tick()
        {
            switch(progress)
            {
                default:
                    End();
                    break;
            }
        }
    }
}
