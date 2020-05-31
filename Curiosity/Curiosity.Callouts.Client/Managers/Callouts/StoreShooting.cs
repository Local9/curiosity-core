using CitizenFX.Core;
using System;
using System.Collections.Generic;
using Ped = Curiosity.Callouts.Client.Classes.Ped;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class StoreShooting : Callout
    {
        List<Ped> Shooters = new List<Ped>();
        List<Ped> Hostages = new List<Ped>();

        private StoreData Store;

        public StoreShooting(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        internal override void Prepare()
        {
            base.Prepare();



            base.Name = "Store Shooting";
        }

        internal override void Tick()
        {
            switch (progress)
            {
                case 0:
                    End(true);
                    break;
            }
        }

        class StoreData
        {

            public Vector3 Location;
            
        }
    }
}
