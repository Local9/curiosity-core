using CitizenFX.Core;
using Curiosity.Template.Client.Interface.Menus.Data;
using NativeUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Template.Client.Interface.Menus.Creator
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

            lstTops = new UIMenuListItem("Tops", GenerateNumberList(MAX_TOP_VALUE), Cache.Character.Appearance.Top);
            lstPants = new UIMenuListItem("Pants", GenerateNumberList(MAX_PANTS_VALUE), Cache.Character.Appearance.Pants);
            lstShoes = new UIMenuListItem("Shoes", GenerateNumberList(MAX_SHOE_VALUE), Cache.Character.Appearance.Shoes);
            lstHats = new UIMenuListItem("Hats", GenerateNumberList(MAX_HAT_VALUE), Cache.Character.Appearance.Hat);
            lstGlasses = new UIMenuListItem("Glasses", GenerateNumberList(MAX_GLASSES_VALUE), Cache.Character.Appearance.Glasses);

            Randomise();

            menu.AddItem(lstTops);
            menu.AddItem(lstPants);
            menu.AddItem(lstShoes);
            menu.AddItem(lstHats);
            menu.AddItem(lstGlasses);

            menu.AddInstructionalButton(CreatorMenus.btnRotateLeft);
            menu.AddInstructionalButton(CreatorMenus.btnRotateRight);
            menu.AddInstructionalButton(CreatorMenus.btnRandom);

            return menu;
        }

        private void Randomise()
        {
            Cache.Character.Appearance.Top = CuriosityPlugin.Rand.Next(MAX_TOP_VALUE);
            Cache.Character.Appearance.Pants = CuriosityPlugin.Rand.Next(MAX_PANTS_VALUE);
            Cache.Character.Appearance.Shoes = CuriosityPlugin.Rand.Next(MAX_SHOE_VALUE);
            Cache.Character.Appearance.Hat = CuriosityPlugin.Rand.Next(MAX_HAT_VALUE);
            Cache.Character.Appearance.Glasses = CuriosityPlugin.Rand.Next(MAX_GLASSES_VALUE);

            CharacterClothing.SetPedTop(Game.PlayerPed, Cache.Character.Appearance.Top);
            CharacterClothing.SetPedPants(Game.PlayerPed, Cache.Character.Appearance.Pants);
            CharacterClothing.SetPedShoes(Game.PlayerPed, Cache.Character.Appearance.Shoes);
            CharacterClothing.SetPedHat(Game.PlayerPed, Cache.Character.Appearance.Hat);
            CharacterClothing.SetPedGlasses(Game.PlayerPed, Cache.Character.Appearance.Glasses);

            lstTops.Index = Cache.Character.Appearance.Top;
            lstPants.Index = Cache.Character.Appearance.Pants;
            lstShoes.Index = Cache.Character.Appearance.Shoes;
            lstHats.Index = Cache.Character.Appearance.Hat;
            lstGlasses.Index = Cache.Character.Appearance.Glasses;
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == lstTops)
            {
                CharacterClothing.SetPedTop(Game.PlayerPed, newIndex);
                Cache.Character.Appearance.Top = newIndex;
            }

            if (listItem == lstPants)
            {
                CharacterClothing.SetPedPants(Game.PlayerPed, newIndex);
                Cache.Character.Appearance.Pants = newIndex;
            }

            if (listItem == lstShoes)
            {
                CharacterClothing.SetPedShoes(Game.PlayerPed, newIndex);
                Cache.Character.Appearance.Shoes = newIndex;
            }

            if (listItem == lstHats)
            {
                CharacterClothing.SetPedHat(Game.PlayerPed, newIndex);
                Cache.Character.Appearance.Hat = newIndex;
            }

            if (listItem == lstGlasses)
            {
                CharacterClothing.SetPedGlasses(Game.PlayerPed, newIndex);
                Cache.Character.Appearance.Glasses = newIndex;
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

            if (Game.IsControlJustPressed(0, Control.Jump))
            {
                Randomise();
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
