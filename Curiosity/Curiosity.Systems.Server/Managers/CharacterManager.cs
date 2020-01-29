using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;

namespace Curiosity.Systems.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("character:load", new EventCallback(metadata =>
            {
                Logger.Debug($"[CharacterManager] character:load called");
                return null;
            }));
        }
    }
}
