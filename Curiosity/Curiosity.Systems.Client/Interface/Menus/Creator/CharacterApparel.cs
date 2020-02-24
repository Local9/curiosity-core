using CitizenFX.Core;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus.Creator
{
    class CharacterApparel
    {
        private UIMenuListItem lstTops;
        private UIMenuListItem lstPants;
        private UIMenuListItem lstShoes;
        private UIMenuListItem lstHats;
        private UIMenuListItem lstGlasses;

        private const int MAX_TOP_VALUE = 15;
        private const int MAX_PANTS_VALUE = 12;
        private const int MAX_SHOE_VALUE = 12;
        private const int MAX_HAT_VALUE = 12;
        private const int MAX_GLASSES_VALUE = 5;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;
            menu.OnListChange += Menu_OnListChange;

            lstTops = new UIMenuListItem("Tops", GenerateNumberList(MAX_TOP_VALUE), 0);
            lstPants = new UIMenuListItem("Pants", GenerateNumberList(MAX_PANTS_VALUE), 0);
            lstShoes = new UIMenuListItem("Shoes", GenerateNumberList(MAX_SHOE_VALUE), 0);
            lstHats = new UIMenuListItem("Hats", GenerateNumberList(MAX_HAT_VALUE), 0);
            lstGlasses = new UIMenuListItem("Glasses", GenerateNumberList(MAX_GLASSES_VALUE), 0);

            menu.AddInstructionalButton(CreatorMenus.btnRotateLeft);
            menu.AddInstructionalButton(CreatorMenus.btnRotateRight);

            return menu;
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {   
            if (listItem == lstTops)
            {
                CharacterClothing.SetPedTop(Game.PlayerPed, newIndex);
            }

            if (listItem == lstPants)
            {
                CharacterClothing.SetPedPants(Game.PlayerPed, newIndex);
            }

            if (listItem == lstShoes)
            {
                CharacterClothing.SetPedShoes(Game.PlayerPed, newIndex);
            }

            if (listItem == lstHats)
            {
                CharacterClothing.SetPedHat(Game.PlayerPed, newIndex);
            }

            if (listItem == lstGlasses)
            {
                CharacterClothing.SetPedGlasses(Game.PlayerPed, newIndex);
            }
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

        public static List<dynamic> GenerateNumberList(int max, int min = 0)
        {
            List<dynamic> lst = new List<dynamic>();
            for (int i = min; i < max + 1; i++)
                lst.Add($"#{i.ToString()}/{max}");
            return lst;
        }
    }
}
