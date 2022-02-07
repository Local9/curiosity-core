using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Managers.GameWorld;
using NativeUI;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.PoliceSubMenu
{
    internal class PolicePlayerListMenu
    {
        UIMenu _menu;
        WorldPlayerManager worldPlayerManager => WorldPlayerManager.GetModule();

        public UIMenu CreateMenu(UIMenu m)
        {
            _menu = m;

            _menu.OnMenuStateChanged += _menu_OnMenuStateChanged;

            return _menu;
        }

        private void _menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward || state == MenuState.Opened)
            {
                _menu.Clear();

                foreach (KeyValuePair<int, WorldPlayer> kvp in worldPlayerManager.WorldPlayers)
                {
                    WorldPlayer worldPlayer = kvp.Value;
                    Player player = kvp.Value.Player;

                    if (player == Game.Player) continue; // ignore self
                    if (!Game.PlayerPed.IsInRangeOf(player.Character.Position, 10f)) continue;

                    if (player.Character.IsInVehicle()) continue; // they must be outside a vehicle

                    if (!worldPlayer.IsWanted) continue;

                    bool hasLos = HasEntityClearLosToEntity(Game.PlayerPed.Handle, player.Character.Handle, 17);
                    if (!hasLos) continue;

                    UIMenu uIMenu = InteractionMenu.MenuPool.AddSubMenu(_menu, player.Name);
                    new PolicePlayerInteractionMenu().CreateMenu(uIMenu, player.ServerId);
                }

            }
        }
    }
}
