using CitizenFX.Core;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using System;

namespace Curiosity.Core.Client.ClientExports
{
    public class PlayerExport : Manager<PlayerExport>
    {
        public override void Begin()
        {
            Instance.ExportDictionary.Add("Passive", new Func<bool>(
                () =>
                {
                    return Game.Player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
                }
            ));
        }
    }
}
