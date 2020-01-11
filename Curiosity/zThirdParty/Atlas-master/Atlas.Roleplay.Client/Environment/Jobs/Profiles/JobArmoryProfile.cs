using System;
using System.Drawing;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Package;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment.Jobs.Profiles
{
    public class JobArmoryProfile : JobProfile
    {
        public override JobProfile[] Dependencies { get; set; } = {new JobStorageProfile()};
        public Position Armory { get; set; }
        public Position MerchantPosition { get; set; }
        public int MerchantModel { get; set; }
        public Merchant Merchant { get; set; }
        public Action Callback { get; set; }

        public override async void Begin(Job job)
        {
            var model = new Model(MerchantModel);

            if (MerchantModel != 0 && MerchantPosition != null)
            {
                var merchantId = $"merchant::{job.Attachment.ToString().ToLower()}";
                var package = NetworkPackage.GetModule();

                if (Session.IsSpawnHost() && !API.NetworkDoesNetworkIdExist(package.GetLoad<int>(merchantId).Get()))
                {
                    await model.Request(5000);

                    var ped = API.CreatePed(6, (uint) model.Hash, MerchantPosition.X, MerchantPosition.Y,
                        MerchantPosition.Z, MerchantPosition.Heading, true, false);

                    Merchant = new Merchant(ped)
                    {
                        Important = true,
                        Position = MerchantPosition
                    };

                    package.GetLoad<int>(merchantId).UpdateAndCommit(Merchant.NetworkModule.GetId());

                    await SafeTeleport.Teleport(ped, MerchantPosition);

                    Merchant.Movable = false;
                }
                else
                {
                    while ((Merchant = MerchantManager.GetModule()
                               .GetMerchantById(package.GetLoad<int>(merchantId).Get())) == null)
                    {
                        await BaseScript.Delay(1000);
                    }
                }
            }

            await Session.Loading();

            var player = Cache.Player;
            var character = Cache.Character;
            var marker = new Marker(Armory)
            {
                Message = "Tryck ~INPUT_CONTEXT~ för att komma åt förrådet",
                Color = Color.Transparent,
                Condition = self => character.Metadata.Employment == job.Attachment
            };

            marker.Callback += async () =>
            {
                if (MerchantPosition != null && Merchant != null)
                {
                    await player.Entity.AnimationQueue.PlayDirectInQueue(new AnimationBuilder()
                        .Select("gestures@m@standing@casual", "gesture_hello")
                        .AtPosition(Armory)
                    );
                }

                Callback?.Invoke();
            };

            marker.Show();

            Atlas.AttachTickHandler(OnTick);
        }

        private async Task OnTick()
        {
            if (Merchant != null && Merchant.Position.Distance(MerchantPosition) > 1f)
            {
                await SafeTeleport.Teleport(Merchant.Id, MerchantPosition.Add(new Position(0, 0, 0.5f)), 100);
            }

            await BaseScript.Delay(500);
        }
    }
}