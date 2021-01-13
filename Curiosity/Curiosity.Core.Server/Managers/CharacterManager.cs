using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("character:load", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                curiosityUser.Character = await Database.Store.CharacterDatabase.Get(curiosityUser.DiscordId);

                return curiosityUser.Character;
            }));

            EventSystem.GetModule().Attach("character:save", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityCharacter curiosityCharacter = metadata.Find<CuriosityCharacter>(0);

                await curiosityCharacter.Save();

                return null;
            }));
        }
    }
}
