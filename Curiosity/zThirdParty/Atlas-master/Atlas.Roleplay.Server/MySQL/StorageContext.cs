using Atlas.Roleplay.Library.Billing;
using Atlas.Roleplay.Library.Inventory;
using Atlas.Roleplay.Library.Models;
using System.Data.Entity;

namespace Atlas.Roleplay.Server.MySQL
{
    public class StorageContext : DatabaseContext<StorageContext>
    {
        public DbSet<AtlasUser> Users { get; set; }
        public DbSet<AtlasCharacter> Characters { get; set; }
        public DbSet<Storage> Storages { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Business> Businesses { get; set; }

        protected override void OnModelCreating(DbModelBuilder model)
        {
            model.Entity<AtlasUser>()
                .ToTable("users")
                .Ignore(self => self.Handle)
                .Ignore(self => self.ConnectionHistory)
                .Ignore(self => self.Characters)
                .Ignore(self => self.Metadata);

            model.Entity<AtlasCharacter>()
                .ToTable("characters")
                .Ignore(self => self.Style)
                .Ignore(self => self.BankAccount)
                .Ignore(self => self.Metadata);

            model.Entity<Bill>()
                .Ignore(self => self.Receiver)
                .Ignore(self => self.Sender)
                .Ignore(self => self.Designation)
                .Ignore(self => self.AmountLines)
                .Ignore(self => self.IsCreated);

            model.Entity<Storage>()
                .Ignore(self => self.Metadata);

            model.Entity<Business>()
                .ToTable("businesses");
        }

        public DbContextTransaction BeginTransaction()
        {
            return Database.BeginTransaction();
        }
    }
}