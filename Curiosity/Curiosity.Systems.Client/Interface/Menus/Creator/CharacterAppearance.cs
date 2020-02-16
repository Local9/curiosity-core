using CitizenFX.Core;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus.Creator
{
    class CharacterAppearance
    {
        private UIMenuListItem lstHair; // Color Panel
        private UIMenuListItem lstEyebrows; // Color Panel + Opacity
        private UIMenuListItem lstFacialHair; // Male Only | Color Panel + Opacity
        private UIMenuListItem lstSkinBlemishes; // Opacity
        private UIMenuListItem lstSkinAging; // Opacity
        private UIMenuListItem lstSkinComplexion; // Opacity
        private UIMenuListItem lstSkinMoles; // Opacity
        private UIMenuListItem lstSkinDamage; // Opacity
        private UIMenuListItem lstEyeColor;
        private UIMenuListItem lstEyeMakeup; // Opacity
        private UIMenuListItem lstBlusher; // Female Only | Color Panel + Opacity
        private UIMenuListItem lstLipstick; // Color Panel + Opacity

        private int Hair;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            menu.AddInstructionalButton(CreatorMenus.btnRotateLeft);
            menu.AddInstructionalButton(CreatorMenus.btnRotateRight);

            return menu;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);
        }

        private async Task OnPlayerControls()
        {
            CreatorMenus._MenuPool.ProcessMouse();

            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Game.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Game.PlayerPed.Heading -= 10f;
            }
        }
    }
}
