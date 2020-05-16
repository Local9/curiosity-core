using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Inventory;
using Atlas.Roleplay.Server.MySQL;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Atlas.Roleplay.Server.Managers
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