using CitizenFX.Core;
using CitizenFX.Core.Native;
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

        private List<string> hairStyleList = new List<string>();
        private List<string> eyebrowsStyleList = new List<string>();
        private List<string> facialHairStylesList = new List<string>();
        private List<string> blemishesStyleList = new List<string>();
        private List<string> ageingStyleList = new List<string>();
        private List<string> complexionStyleList = new List<string>();
        private List<string> molesFrecklesStyleList = new List<string>();
        private List<string> skinDamageStyleList = new List<string>();
        private List<string> eyeColorList = new List<string>();
        private List<string> eyeMakeupStyleList = new List<string>();
        private List<string> blusherStyleList = new List<string>();
        private List<string> lipstickStyleList = new List<string>();
        
        /* vMENU
           0               Blemishes             0 - 23,   255  
           1               Facial Hair           0 - 28,   255  
           2               Eyebrows              0 - 33,   255  
           3               Ageing                0 - 14,   255  
           4               Makeup                0 - 74,   255  
           5               Blush                 0 - 6,    255  
           6               Complexion            0 - 11,   255  
           7               Sun Damage            0 - 10,   255  
           8               Lipstick              0 - 9,    255  
           9               Moles/Freckles        0 - 17,   255  
           10              Chest Hair            0 - 16,   255  
           11              Body Blemishes        0 - 11,   255  
           12              Add Body Blemishes    0 - 1,    255  

           */

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            menu.AddInstructionalButton(CreatorMenus.btnRotateLeft);
            menu.AddInstructionalButton(CreatorMenus.btnRotateRight);

            return menu;
        }

        private void Menu_OnMenuClose(UIMenu menu)
        {
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);
        }

        private void Menu_OnMenuOpen(UIMenu menu)
        {
            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);

            menu.Clear(); // clear the menu on load as the gender may of changed

            menu.AddItem(lstHair);
            menu.AddItem(lstEyebrows);
            
            if (Game.PlayerPed.Gender == Gender.Male)
                menu.AddItem(lstFacialHair);
            
            menu.AddItem(lstSkinBlemishes);
            menu.AddItem(lstSkinAging);
            menu.AddItem(lstSkinComplexion);
            menu.AddItem(lstSkinMoles);
            menu.AddItem(lstSkinDamage);
            menu.AddItem(lstEyeColor);
            menu.AddItem(lstEyeMakeup);
            
            if (Game.PlayerPed.Gender == Gender.Female)
                menu.AddItem(lstBlusher);

            menu.AddItem(lstLipstick);
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

        private void CreateMenuItems()
        {
            int maxHairStyles = API.GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2);
            for (int i = 0; i < maxHairStyles; i++)
            {
                
            }
        }
    }
}
