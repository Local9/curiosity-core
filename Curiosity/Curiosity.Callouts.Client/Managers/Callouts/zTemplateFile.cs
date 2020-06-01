﻿using CitizenFX.Core;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class Template : Callout
    {
        public Template(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        internal async override void Prepare()
        {
            base.Prepare();
        }
        internal override void End(bool forcefully = false)
        {
            base.End(forcefully);
        }

        internal override void Tick()
        {
            throw new NotImplementedException();
        }
    }
}
