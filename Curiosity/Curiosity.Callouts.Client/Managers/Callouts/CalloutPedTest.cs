using CitizenFX.Core;
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
    internal class CalloutPedTest : Callout
    {
        private CalloutMessage calloutMessage = new CalloutMessage();

        Ped Ped;

        public CalloutPedTest(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        internal async override void Prepare()
        {
            base.Prepare();

            Vector3 position = Players[0].Character.GetOffsetPosition(new Vector3(2f, 0f, 0f));

            Ped = await Ped.Spawn(PedHash.Tourist01AFM, position, false);

            if (Ped != null)
                Ped.IsArrestable = true;

            base.IsSetup = true;
        }

        internal override void End(bool forcefully = false, CalloutMessage cm = null)
        {
            cm = calloutMessage;
            base.End(forcefully, cm);
        }

        internal async override void Tick()
        {
            switch(progress)
            {
                case 1:
                    progress = 2;
                    break;
            }
        }
    }
}
