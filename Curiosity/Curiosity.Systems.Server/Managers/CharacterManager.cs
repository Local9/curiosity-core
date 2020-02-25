using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
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
                CuriosityUser curiosityUser = CuriosityPlugin.ActiveUsers[player.Handle];

                curiosityUser.Character = await MySQL.Store.CharacterDatabase.Get(player, curiosityUser.DiscordId);

                return curiosityUser.Character;
            }));

            EventSystem.GetModule().Attach("character:save", new AsyncEventCallback(async metadata =>
            {
                CuriosityCharacter curiosityCharacter = metadata.Find<CuriosityCharacter>(0);
                await curiosityCharacter.Save();
                return null;
            }));
        }
    }
}
