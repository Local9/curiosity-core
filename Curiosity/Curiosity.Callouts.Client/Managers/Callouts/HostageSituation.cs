using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Managers.Callouts.Data;
using Curiosity.Callouts.Shared.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ped = Curiosity.Callouts.Client.Classes.Ped;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class HostageSituation : Callout
    {
        List<Ped> Shooters = new List<Ped>();
        List<Ped> Hostages = new List<Ped>();

        private HostageDataModel data;

        private List<PedHash> CityPedHashes = new List<PedHash>()
        {
            PedHash.ChiBoss01GMM,
            PedHash.ChiGoon01GMM,
            PedHash.ChiGoon02GMM,
            PedHash.Dockwork01SMM,
            PedHash.Dockwork01SMY,
            PedHash.Dealer01SMY,
            PedHash.BikerChic,
            PedHash.Lost01GFY,
            PedHash.Eastsa01AMM,
            PedHash.Eastsa01AFY,
            PedHash.Eastsa03AFY,
            PedHash.AlDiNapoli,
        };

        private List<PedHash> RurualPedHashes = new List<PedHash>()
        {
            PedHash.StrPunk01GMY,
            PedHash.StrPunk02GMY,
            PedHash.Surfer01AMY,
            PedHash.Stwhi02AMY,
            PedHash.Farmer01AMM,
            PedHash.Acult01AMM,
            PedHash.Acult01AMO,
            PedHash.Acult01AMY,
            PedHash.Acult02AMO,
            PedHash.Acult02AMY,
        };

        private List<PedHash> CountryPedHashes = new List<PedHash>()
        {
            PedHash.Hillbilly01AMM,
            PedHash.Hillbilly02AMM,
            PedHash.Farmer01AMM,
            PedHash.Hippie01AFY,
            PedHash.Hippy01AMY,
            PedHash.Lost01GMY,
            PedHash.Lost02GMY,
            PedHash.Hippie01,
        };

        private List<WeaponHash> weaponHashes = new List<WeaponHash>()
        {
            WeaponHash.Pistol,
            WeaponHash.SMG,
            WeaponHash.MiniSMG,
            WeaponHash.PumpShotgun,
            WeaponHash.SawnOffShotgun,
            WeaponHash.SniperRifle,
        };

        public HostageSituation(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        internal override async void Prepare()
        {
            base.Prepare();

            Ped ped = await Ped.Spawn(PedHash.Abigail, Game.PlayerPed.GetOffsetPosition(new Vector3(1f, 0f, 0f)));
            Hostages.Add(ped);

            RegisterPed(ped);

            base.IsSetup = true;
        }

        internal override void Tick()
        {
            int numberOfAliveHostages = Hostages.Select(x => x).Where(x => x.IsAlive).Count();

            Screen.ShowSubtitle($"No. {numberOfAliveHostages}, P: {progress}");

            switch (progress)
            {
                case 0:
                    End(true);
                    break;
                case 1:
                    

                    if (numberOfAliveHostages == 0)
                    {
                        progress = 0;
                    }

                    break;
            }
        }

        internal override void End(bool forcefully = false)
        {
            base.End(forcefully);

            Screen.ShowNotification($"Callout Ended");
        }

    }
}
