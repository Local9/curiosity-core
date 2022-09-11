using Curiosity.Framework.Server.Events;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;

namespace Curiosity.Framework.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public override void Begin()
        {
            EventDispatcher.Mount("character:save", new Func<ClientId, Character, Task<bool>>(OnSaveCharacterAsync));
        }

        private async Task<bool> OnSaveCharacterAsync(ClientId client, Character character)
        {
            Logger.Debug($"{character.Stats.STAMINA}");
            return true;
        }
    }
}
