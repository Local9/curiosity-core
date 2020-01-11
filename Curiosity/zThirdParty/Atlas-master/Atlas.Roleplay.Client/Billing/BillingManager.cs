using System.Collections.Generic;
using System.Linq;
using Atlas.Roleplay.Client.Diagnostics;
using Atlas.Roleplay.Client.Environment.Jobs;
using Atlas.Roleplay.Client.Environment.Jobs.Profiles;
using Atlas.Roleplay.Client.Events;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Billing;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Atlas.Roleplay.Client.Billing
{
    public class BillingManager : Manager<BillingManager>
    {
        public override void Begin()
        {
            Atlas.AttachNuiHandler("CLOSE_BILL", new EventCallback(metadata =>
            {
                API.SetNuiFocus(false, false);

                return null;
            }));

            Atlas.AttachNuiHandler("PAY_BILL", new AsyncEventCallback(async metadata =>
            {
                var player = Cache.Player;
                var character = player.Character;
                var seed = metadata.Find<string>(0);

                var bills = await EventSystem.Request<List<Bill>>("billing:fetch", Cache.Character.Seed);
                var bill = bills.FirstOrDefault(self => self.Seed == seed);

                API.SetNuiFocus(false, false);

                if (bill == null)
                {
                    Logger.Info($"[Billing] Could not pay bill due to it already being removed. ({seed})");

                    return null;
                }

                if (character.BankAccount.Balance >= bill.Amount)
                {
                    character.BankAccount.Balance -= bill.Amount;

                    var job = JobManager.GetModule().Registered
                        .FirstOrDefault(self => self.Label == bill.Sender.Business);
                    var profile = job?.GetProfile<JobBusinessProfile>();

                    if (profile != null)
                    {
                        profile.Business.Balance += bill.Amount;
                        profile.Commit();
                    }

                    player.ShowNotification($"Du betalade faktura #{bill.BillNumber}");

                    EventSystem.GetModule().Send("PAY_BILL", seed);
                }
                else
                {
                    player.ShowNotification($"Du har inte råd att betala denna faktura. ({bill.Amount} SEK)");
                }

                return null;
            }));

            Atlas.AttachNuiHandler("DESTROY_BILL", new EventCallback(metadata =>
            {
                API.SetNuiFocus(false, false);

                EventSystem.GetModule().Send("DESTROY_BILL", metadata.Find<string>(0));

                return null;
            }));

            Atlas.AttachNuiHandler("SUBMIT_BILL", new AsyncEventCallback(async metadata =>
            {
                var player = Cache.Player;
                var bill = JsonConvert.DeserializeObject<Bill>(metadata.Find<string>(0));

                bill.IsCreated = true;
                bill.IsActive = true;

                API.SetNuiFocus(false, false);

                if (bill.Receiver.Type == BillReceiverType.Individual)
                {
                    var character = await EventSystem.Request<AtlasCharacter>("characters:fetch", bill.Receiver.Name);

                    if (character == null)
                    {
                        player.ShowNotification($"Kunde inte hitta någon vid namn {bill.Receiver.Name}.");

                        return null;
                    }

                    var user = await EventSystem.Request<AtlasUser>("user:fetch", character.Owner);

                    user?.Send("billing:receive", bill);
                }
                else
                {
                    var job = JobManager.GetModule().Registered.FirstOrDefault(self =>
                        self.Label.ToLower().Trim() == bill.Receiver.Name.ToLower().Trim());

                    if (job == null)
                    {
                        player.ShowNotification($"Kunde inte hitta ett företag vid namn {bill.Receiver.Name}.");

                        return null;
                    }

                    player.ShowNotification("Denna funktion är för tillfället inaktiverad.");

                    return null;
                }

                EventSystem.GetModule().Send("billing:create", bill);

                Cache.Player.ShowNotification(
                    $"{bill.Sender.Business}: Skickade en faktura till {bill.Receiver.Name}. (Att betala: {bill.Amount} SEK)");

                return null;
            }));

            EventSystem.GetModule().Attach("billing:receive", new EventCallback(metadata =>
            {
                var bill = metadata.Find<Bill>(0);

                if (bill.Receiver.Name == Cache.Character.Fullname)
                    Cache.Player.ShowNotification(
                        $"{bill.Sender.Business}: Du fick en faktura av {bill.Receiver.Name}. (Att betala: {bill.Amount} SEK)");

                return null;
            }));
        }

        public Bill CreateBill(BillSender sender, BillReceiver receiver)
        {
            var bill = new Bill
            {
                Sender = sender,
                Receiver = receiver
            };

            bill.LookAt();

            return bill;
        }
    }
}