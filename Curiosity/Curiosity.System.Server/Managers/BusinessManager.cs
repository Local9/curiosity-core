using Curiosity.System.Library.Events;
using Curiosity.System.Library.Models;
using Curiosity.System.Server.Events;
using Curiosity.System.Server.Extensions;
using Curiosity.System.Server.MySQL;
using System.Linq;

namespace Curiosity.System.Server.Managers
{
    public class BusinessManager : Manager<BusinessManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("business:update", new AsyncEventCallback(async metadata =>
            {
                var business = metadata.Find<Business>(0);

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    context.Businesses.AddOrUpdate(business);

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }

                Curiosity.ActiveUsers.Where(self => self.Handle != metadata.Sender).ToList()
                    .ForEach(self => self.Send("business:update", business));

                return null;
            }));

            EventSystem.GetModule().Attach("business:fetch", new EventCallback(metadata =>
            {
                var seed = metadata.Find<string>(0);

                using (var context = new StorageContext())
                {
                    foreach (var business in context.Businesses)
                    {
                        if (business.Seed != seed) continue;

                        return business;
                    }
                }

                return null;
            }));
        }
    }
}