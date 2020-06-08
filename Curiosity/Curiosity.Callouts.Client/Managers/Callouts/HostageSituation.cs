using CitizenFX.Core;
using CitizenFX.Core.Native;
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
using static Curiosity.Callouts.Client.Classes.Ped;
using Ped = Curiosity.Callouts.Client.Classes.Ped;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class HostageSituation : Callout
    {
        private PluginManager PluginInstance => PluginManager.Instance;

        List<Ped> Shooters = new List<Ped>();
        List<Ped> Hostages = new List<Ped>();
        Blip Blip;

        CalloutMessage calloutMessage = new CalloutMessage();

        private HostageDataModel data;
        private PedHash lastPedHash;
        private float spawnRadius;

        private int hostageReleaseTracker = 0;
        private int numberOfHostages = 0;
        private int numberOfShooters = 0;

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

            List<HostageDataModel> hostageDataList = Data.HostageData.Situations.Select(x => x).Where(x => x.PatrolZone == PlayerManager.PatrolZone).ToList();

            if (hostageDataList.Count == 0)
            {
                Logger.Log($"No hostage situations returned for '{PlayerManager.PatrolZone}'");
                base.End(true);
                return;
            }

            data = hostageDataList.Random();

            spawnRadius = data.SpawnRadius > 200f ? 100f : data.SpawnRadius;

            Blip = World.CreateBlip(data.Location);
            Blip.Sprite = BlipSprite.BigCircle;
            Blip.Color = BlipColor.Yellow;
            Blip.ShowRoute = true;
            Blip.IsShortRange = true;
            Blip.Alpha = 120;
            Blip.Scale = data.BlipScale;

            numberOfHostages = data.Hostages.Count();
            numberOfShooters = data.Shooters.Count();

            calloutMessage.CalloutType = CalloutType.HOSTAGE_RESCUE;

            PluginInstance.RegisterTickHandler(OnHostageMessagePrompt);

            UiTools.Dispatch("CODE 3", "Hostage Situation~n~~r~Shots fired! Shots fired!");

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

                Ped ped = await Ped.Spawn(pedHash, h.Item1, false);
                ped.Heading = h.Item2;

                RelationshipGroup relationshipGroup = (uint)Collections.RelationshipHash.Civfemale;
                if (ped.Fx.Gender == Gender.Male)
                {
                    relationshipGroup = (uint)Collections.RelationshipHash.Civmale;
                }

                ped.RunSequence(Ped.Sequence.KNEEL);

                ped.IsHostage = true;
                ped.IsImportant = true;
                ped.IsArrestable = false;

                ped.Fx.RelationshipGroup = relationshipGroup;

                RegisterPed(ped);
                Hostages.Add(ped);
            });
        }

        private void SetupShooters(List<Tuple<Vector3, float>> shooters)
        {
            shooters.ForEach(async s =>
            {
                PedHash pedHash = CityPedHashes.Random();

                // Look into Ocean and Highway

                switch (PlayerManager.PatrolZone)
                {
                    case PatrolZone.City:
                        pedHash = CityPedHashes.Random();
                        break;
                    case PatrolZone.Country:
                        pedHash = CountryPedHashes.Random();
                        break;
                    case PatrolZone.Rural:
                        pedHash = RurualPedHashes.Random();
                        break;
                    default:
                        pedHash = CityPedHashes.Random();
                        break;
                }

                while (pedHash == lastPedHash)
                {
                    pedHash = HostagePedHashes.Random();
                }

                lastPedHash = pedHash;

                Ped ped = await Ped.Spawn(pedHash, s.Item1, false);
                ped.Heading = s.Item2;

                RelationshipGroup relationshipGroup = (uint)Collections.RelationshipHash.Gang1;
                ped.Fx.RelationshipGroup = relationshipGroup;
                relationshipGroup.SetRelationshipBetweenGroups(Game.PlayerPed.RelationshipGroup, Relationship.Hate, true);
                ped.Task.FightAgainstHatedTargets(data.SpawnRadius);

                ped.IsMission = true;
                ped.IsImportant = true;
                ped.IsArrestable = true;
                ped.IsSuspect = true;

                Decorators.Set(ped.Handle, Decorators.PED_MISSION, true);

                ped.Fx.Weapons.Give(weaponHashes.Random(), 90, true, true);
                ped.Fx.DropsWeaponsOnDeath = false;

                RegisterPed(ped);
                Shooters.Add(ped);
            });
        }

        internal override async void Tick()
        {
            int numberOfAliveHostages = Hostages.Select(x => x).Where(x => x.IsAlive).Count();
            int numberOfReleasedHostages = Hostages.Select(x => x).Where(x => x.IsReleased).Count();
            int numberOfAliveShooters = Shooters.Select(x => x).Where(x => x.IsAlive).Count();

            int numberOfAlivePlayers = Players.Select(x => x).Where(x => x.IsAlive).Count();

            if (numberOfAlivePlayers == 0) // clear callout
            {
                End(true);
            }

            if (Game.PlayerPed.Position.Distance(data.Location) < 50f)
            {
                if (Blip != null)
                {
                    if (Blip.Exists())
                        Blip.Delete();
                }
            }

            

#if DEBUG
            if (PlayerManager.IsDeveloper)
                Screen.ShowSubtitle($"H. {numberOfAliveHostages}, HRT: {hostageReleaseTracker}, S: {numberOfAliveShooters}, P: {progress}");
#endif

            switch (progress)
            {
                case 1:
                    if (Game.PlayerPed.Position.Distance(data.Location) > spawnRadius) return;
                    progress++;
                    break;
                case 2:
                    SetupHostages(data.Hostages);
                    SetupShooters(data.Shooters);
                    progress++;
                    break;
                case 3:
                    if (numberOfAliveShooters > 0) return;
                    Screen.ShowNotification($"All Suspects have been killed. Remember to save the hostages");
                    progress++;
                    break;
                case 4:
                    if (Game.PlayerPed.Position.Distance(data.Location) > 200f)
                    {
                        API.ClearAreaOfEverything(data.Location.X, data.Location.Y, data.Location.Z, 50f, false, false, false, false);
                        progress++;
                    }
                    break;
            }
        }

        internal async Task OnHostageMessagePrompt()
        {
            Ped ped = Hostages.Select(x => x).Where(x => x.Position.Distance(Game.PlayerPed.Position) < 1.5f).FirstOrDefault();

            if (ped == null) return;

            string message = $"Press ~INPUT_CONTEXT~ to release";
            Screen.DisplayHelpTextThisFrame($"{message}");

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                ped.RunSequence(Sequence.UNKNEEL_AND_FLEE);
                Decorators.Set(ped.Fx.Handle, Decorators.PED_RELEASED, true);
                hostageReleaseTracker++;

                int remaining = numberOfHostages - hostageReleaseTracker;

                if (remaining == 0)
                {
                    Screen.ShowNotification($"You have released a hostage. No more hostages remaining.");
                }
                else
                {
                    Screen.ShowNotification($"You have released a hostage. {remaining} hostages remaining.");
                }
            }
        }

        internal override async void End(bool forcefully = false, CalloutMessage cm = null)
        {
            PluginInstance.DeregisterTickHandler(OnHostageMessagePrompt);

            Hostages.Clear();
            Shooters.Clear();

            calloutMessage.Success = true;
            calloutMessage.NumberRescued = hostageReleaseTracker;

            cm = calloutMessage;
            base.End(forcefully, cm);

            cm = null;
        }

    }
}
