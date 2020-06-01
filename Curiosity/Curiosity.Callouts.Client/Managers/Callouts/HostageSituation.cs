using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Managers.Callouts.Data;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ped = Curiosity.Callouts.Client.Classes.Ped;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class HostageSituation : Callout
    {
        List<Ped> Shooters = new List<Ped>();
        List<Ped> Hostages = new List<Ped>();

        CalloutMessage calloutMessage = new CalloutMessage();

        private HostageDataModel data;
        private PedHash lastPedHash;

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

        private List<PedHash> HostagePedHashes = new List<PedHash>()
        {
            PedHash.Abigail,
            PedHash.ShopKeep01,
            PedHash.Tourist01AFM,
            PedHash.Tourist01AFY,
            PedHash.Tourist01AMM,
            PedHash.Tourist02AFY,
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
            progress = 1;

            // Get a dataset based on patrolZone

            data = new HostageDataModel();

            Vector3 offset = Game.PlayerPed.GetOffsetPosition(new Vector3(1f, 0f, 0f));

            Tuple<Vector3, float> tuple = new Tuple<Vector3, float>(offset, 0f);

            data.Location = Game.PlayerPed.Position; // testing
            data.MissionRadius = 10f;

            // data.Hostages.Add(tuple);
            data.Hostages.Add(tuple);

            if (data != null)
            {
                SetupShooters(data.Shooters);
                SetupHostages(data.Hostages);
            }

            base.IsSetup = true;
        }

        private void SetupHostages(List<Tuple<Vector3, float>> hostageList)
        {
            hostageList.ForEach(async h =>
            {
                PedHash pedHash = HostagePedHashes.Random();

                while (pedHash == lastPedHash)
                {
                    pedHash = HostagePedHashes.Random();
                }

                lastPedHash = pedHash;

                Ped ped = await Ped.Spawn(pedHash, h.Item1);
                ped.Heading = h.Item2;
                RegisterPed(ped);
                Hostages.Add(ped);
            });
        }

        private void SetupShooters(List<Tuple<Vector3, float>> shooters)
        {

        }

        internal override async void Tick()
        {
            int numberOfAliveHostages = Hostages.Select(x => x).Where(x => x.IsAlive).Count();

            Screen.ShowSubtitle($"No. {numberOfAliveHostages}, P: {progress}");

            switch (progress)
            {
                case 0:
                    if (calloutMessage.IsCalloutFinished) return;
                    calloutMessage.IsCalloutFinished = true;
                    End();
                    break;
                case 1:

                    await BaseScript.Delay(100);

                    if (numberOfAliveHostages == 0)
                    {
                        calloutMessage.Success = false;
                        progress = 0;
                    }

                    break;
            }
        }

        internal override async void End(bool forcefully = false)
        {
            while (Game.PlayerPed.Position.Distance(data.Location) < data.MissionRadius)
            {
                Screen.DisplayHelpTextThisFrame("Please leave the area");
                await BaseScript.Delay(0);
            }

            Hostages.Clear();
            Shooters.Clear();

            base.End(forcefully);
            base.CompleteCallout(calloutMessage);

            calloutMessage = null;
        }

    }
}
