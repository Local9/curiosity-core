using Curiosity.System.Library.Inventory;
using Curiosity.System.Library.Models;

namespace Curiosity.System.Server.MySQL
{
    public class StorageContext : DatabaseContext<StorageContext>
    {
        public DbSet<CuriosityUser> Users { get; set; }
        public DbSet<CuriosityCharacter> Characters { get; set; }
        public DbSet<Storage> Storages { get; set; }
        public DbSet<Business> Businesses { get; set; }

        protected override void OnModelCreating(DbModelBuilder model)
        {
            model.Entity<CuriosityUser>()
                .ToTable("users")
                .Ignore(self => self.Handle)
                .Ignore(self => self.ConnectionHistory)
                .Ignore(self => self.Characters)
                .Ignore(self => self.Metadata);

            model.Entity<CuriosityCharacter>()
                .ToTable("characters")
                .Ignore(self => self.Style)
                .Ignore(self => self.BankAccount)
                .Ignore(self => self.Metadata);

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