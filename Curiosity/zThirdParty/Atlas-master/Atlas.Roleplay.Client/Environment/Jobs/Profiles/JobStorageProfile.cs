using System.Collections.Generic;
using System.Linq;
using Atlas.Roleplay.Client.Diagnostics;
using Atlas.Roleplay.Client.Events;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Inventory;
using Atlas.Roleplay.Client.Package;
using Atlas.Roleplay.Library.Inventory;
using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Client.Environment.Jobs.Profiles
{
    public class JobStorageProfile : JobProfile
    {
        public override JobProfile[] Dependencies { get; set; }
        public string Seed { get; set; }
        public Storage Storage { get; set; }
        public Position Position { get; set; }

        public override async void Begin(Job job)
        {
            await Session.Loading();
            
            var package = NetworkPackage.GetModule();

            Seed = $"storage::{job.Attachment.ToString().ToLower()}";
            Storage = await EventSystem.GetModule().Request<Storage>("storage:fetch", Seed) ??
                      (package.GetLoad<Storage>(Seed).Get() ?? new Storage
                      {
                          Seed = Seed, Owner = job.Attachment.ToString().ToLower(),
                          Metadata = new StorageMetadata {Items = new List<InventoryItem>()}
                      });

            var inventory = new JobStorageInventory(new InventoryContainerBase
            {
                Seed = Seed,
                Name = $"Förråd ({job.Label})",
                SlotAmount = 100,
                Slots = Storage.Metadata.Items.OrderBy(self => self.Slot).ToArray()
            }, this);

            package.Imports += (__package, index) =>
            {
                if (index != "World.Items.Dropped") return;

                var storage = package.GetLoad<Storage>(Seed).Get();

                if (storage == null) return;

                Storage = storage;

                inventory.Slots = storage.Metadata.Items.OrderBy(self => self.Slot).ToArray();
                inventory.Commit();
            };

            InventoryManager.GetModule().RegisterContainer(inventory);

            Logger.Info(
                $"[Job] [{job.Attachment.ToString()}] Fetched {Storage.Metadata.Items.Count} item(s) in `{Seed}` from the database.");
        }

        public void Commit()
        {
            var storage = NetworkPackage.GetModule().GetLoad<Storage>(Seed);

            storage.Update(Storage);
            storage.Commit();
        }
    }

    public class JobStorageInventory : InventoryContainer
    {
        public JobStorageInventory(InventoryContainerBase inventoryBase, JobStorageProfile parent) : base(inventoryBase)
        {
            Visibility = manager => Cache.Character.Metadata.Employment == parent.Job.Attachment &&
                                    Cache.Entity.Position.Distance(parent.Position) < 2f;

            Update += (item, type) =>
            {
                parent.Storage.Metadata.Items = Slots.Where(self => self != null && self.Seed != "__none").ToList();
                parent.Commit();

                EventSystem.GetModule().Send("storage:update", parent.Storage);
            };
        }
    }
}