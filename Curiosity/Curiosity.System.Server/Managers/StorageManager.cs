using Curiosity.System.Library.Events;
using Curiosity.System.Library.Inventory;
using Curiosity.System.Server.MySQL;
using System.Linq;

namespace Curiosity.System.Server.Managers
{
    public class StorageManager : Manager<StorageManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("storage:fetch", new EventCallback(metadata =>
            {
                var seed = metadata.Find<string>(0);

                using (var context = new StorageContext())
                {
                    return context.Storages.FirstOrDefault(self => self.Seed == seed);
                }
            }));

            EventSystem.Attach("storage:update", new AsyncEventCallback(async metadata =>
            {
                var storage = metadata.Find<Storage>(0);

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    context.Storages.AddOrUpdate(storage);

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }

                return null;
            }));
        }
    }
}