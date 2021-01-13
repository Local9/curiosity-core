using Curiosity.Systems.Library.Models;
using Curiosity.Core.Server.Database.Store;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Extensions
{
    public static class SaveExtensions
    {
        public static async Task Save(this CuriosityUser user)
        {

        }

        public static async Task Save(this CuriosityCharacter character)
        {
            if (character == null) return;

            await CharacterDatabase.Save(character);
        }
    }
}