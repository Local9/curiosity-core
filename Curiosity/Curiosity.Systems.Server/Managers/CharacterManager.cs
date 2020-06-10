using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Events;
using Curiosity.Systems.Server.Extenstions;

namespace Curiosity.Systems.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("character:load", new AsyncEventCallback(async metadata =>
            {
                Player player = CuriosityPlugin.PlayersList[metadata.Sender];
                CuriosityUser curiosityUser = CuriosityPlugin.ActiveUsers[metadata.Sender];

                curiosityUser.Character = await Database.Store.CharacterDatabase.Get(curiosityUser.DiscordId);

                return curiosityUser.Character;
            }));

            EventSystem.GetModule().Attach("character:save", new AsyncEventCallback(async metadata =>
            {
                Player player = CuriosityPlugin.PlayersList[metadata.Sender];
                CuriosityCharacter curiosityCharacter = metadata.Find<CuriosityCharacter>(0);

                if (player.Character.Position != Vector3.Zero)
                    await curiosityCharacter.Save();

                return null;
            }));
        }
    }
}
