using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;

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

                curiosityUser.Character = await MySQL.Store.CharacterDatabase.Get(curiosityUser.License, player);

                return curiosityUser.Character;
            }));
        }
    }
}
