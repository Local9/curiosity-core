using Curiosity.System.Library.Models;
using Curiosity.System.Server.Diagnostics;
using Curiosity.System.Server.MySQL;
using System.Threading.Tasks;

namespace Curiosity.System.Server.Extensions
{
    public static class SaveExtensions
    {
        public static async Task Save(this CuriosityUser user)
        {
            using (var context = new StorageContext())
            using (var transaction = context.BeginTransaction())
            {
                context.Users.AddOrUpdate(user);

                await context.SaveChangesAsync();

                transaction.Commit();

                Logger.Debug(
                    $"[User] [{user.UserId}] Saving `{user.LastName}` last changes and commiting it to `{context.Database.Connection.Database}`.");
            }
        }

        public static async Task Save(this CuriosityCharacter character)
        {
            using (var context = new StorageContext())
            using (var transaction = context.BeginTransaction())
            {
                context.Characters.AddOrUpdate(character);

                await context.SaveChangesAsync();

                transaction.Commit();

                Logger.Debug($"[User] [{character.CharacterId}] - Saving Character and it's assets.");
            }
        }
    }
}