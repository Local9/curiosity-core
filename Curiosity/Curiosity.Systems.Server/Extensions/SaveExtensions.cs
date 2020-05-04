using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Database.Store;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Extenstions
{
    public static class SaveExtensions
    {
        public static async Task Save(this CuriosityUser user)
        {

        }

        public static async Task Save(this CuriosityCharacter character)
        {
            await CharacterDatabase.Save(character);
        }
    }
}