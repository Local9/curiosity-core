using Atlas.Roleplay.Library.Models;
using Atlas.Roleplay.Server.Diagnostics;
using Atlas.Roleplay.Server.MySQL;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Server.Extensions
{
    public static class SaveExtensions
    {
        public static async Task Save(this AtlasUser user)
        {
            using (var context = new StorageContext())
            using (var transaction = context.BeginTransaction())
            {
                context.Users.AddOrUpdate(user);

                await context.SaveChangesAsync();

                transaction.Commit();

                Logger.Debug(
                    $"[User] [{user.Seed}] Saving `{user.LastName}` last changes and commiting it to `{context.Database.Connection.Database}`.");
            }
        }

        public static async Task Save(this AtlasCharacter character)
        {
            using (var context = new StorageContext())
            using (var transaction = context.BeginTransaction())
            {
                context.Characters.AddOrUpdate(character);

                await context.SaveChangesAsync();

                transaction.Commit();

                Logger.Debug($"[User] [{character.Seed}] - Saving `{character.Name} {character.Surname}` and it's assets.");
            }
        }
    }
}