using Atlas.Roleplay.Library.Billing;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Server.MySQL;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Atlas.Roleplay.Server.Managers
{
    public class BillingManager : Manager<BillingManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("billing:create", new AsyncEventCallback(async metadata =>
            {
                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    context.Bills.AddOrUpdate(metadata.Find<Bill>(0));

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }

                return null;
            }));

            EventSystem.Attach("billing:fetch", new EventCallback(metadata =>
            {
                var seed = metadata.Find<string>(0);
                var bills = new List<Bill>();

                using (var context = new StorageContext())
                {
                    var character = context.Characters.First(self => self.Seed == seed);
                    var trimmed = character.Fullname.Trim().ToLower();

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var bill in context.Bills)
                    {
                        if (bill.Sender.Individual.Trim().ToLower() == trimmed ||
                            bill.Receiver.Name.Trim().ToLower() == trimmed)
                        {
                            bills.Add(bill);
                        }
                    }
                }

                return bills;
            }));

            EventSystem.Attach("billing:destroy", new AsyncEventCallback(async metadata =>
            {
                var seed = metadata.Find<string>(0);

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    context.Bills.Remove(context.Bills.First(self => self.Seed == seed));

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }

                return null;
            }));

            EventSystem.Attach("billing:pay", new AsyncEventCallback(async metadata =>
            {
                var seed = metadata.Find<string>(0);

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    var bill = context.Bills.FirstOrDefault(self => self.Seed == seed);

                    if (bill == null) return null;

                    bill.IsActive = false;

                    context.Bills.AddOrUpdate(bill);

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }

                return null;
            }));
        }
    }
}