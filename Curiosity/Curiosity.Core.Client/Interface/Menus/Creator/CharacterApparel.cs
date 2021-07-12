using CitizenFX.Core;
using Curiosity.Core.Client.Interface.Menus.Data;
using NativeUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.Creator
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
            menu.OnListChange += Menu_OnListChange;
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

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

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeBackward)
                OnMenuClose(oldMenu);

            if (state == MenuState.ChangeForward)
                OnMenuOpen(newMenu);
        }

        private void Randomise()
        {
            Cache.Character.Appearance.Top = PluginManager.Rand.Next(MAX_TOP_VALUE);
            Cache.Character.Appearance.Pants = PluginManager.Rand.Next(MAX_PANTS_VALUE);
            Cache.Character.Appearance.Shoes = PluginManager.Rand.Next(MAX_SHOE_VALUE);
            Cache.Character.Appearance.Hat = PluginManager.Rand.Next(MAX_HAT_VALUE);
            Cache.Character.Appearance.Glasses = PluginManager.Rand.Next(MAX_GLASSES_VALUE);

            CharacterClothing.SetPedTop(Cache.PlayerPed, Cache.Character.Appearance.Top);
            CharacterClothing.SetPedPants(Cache.PlayerPed, Cache.Character.Appearance.Pants);
            CharacterClothing.SetPedShoes(Cache.PlayerPed, Cache.Character.Appearance.Shoes);
            CharacterClothing.SetPedHat(Cache.PlayerPed, Cache.Character.Appearance.Hat);
            CharacterClothing.SetPedGlasses(Cache.PlayerPed, Cache.Character.Appearance.Glasses);

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
                CharacterClothing.SetPedTop(Cache.PlayerPed, newIndex);
                Cache.Character.Appearance.Top = newIndex;
            }

            if (listItem == lstPants)
            {
                CharacterClothing.SetPedPants(Cache.PlayerPed, newIndex);
                Cache.Character.Appearance.Pants = newIndex;
            }

            if (listItem == lstShoes)
            {
                CharacterClothing.SetPedShoes(Cache.PlayerPed, newIndex);
                Cache.Character.Appearance.Shoes = newIndex;
            }

            if (listItem == lstHats)
            {
                CharacterClothing.SetPedHat(Cache.PlayerPed, newIndex);
                Cache.Character.Appearance.Hat = newIndex;
            }

            if (listItem == lstGlasses)
            {
                CharacterClothing.SetPedGlasses(Cache.PlayerPed, newIndex);
                Cache.Character.Appearance.Glasses = newIndex;
            }
        }

        private void OnMenuClose(UIMenu menu)
        {
            PluginManager.Instance.DetachTickHandler(OnPlayerControls);

            menu.InstructionalButtons.Clear();
        }

        private void OnMenuOpen(UIMenu menu)
        {
            PluginManager.Instance.AttachTickHandler(OnPlayerControls);

            menu.InstructionalButtons.Clear();

            menu.InstructionalButtons.Add(CreatorMenus.btnRotateLeft);
            menu.InstructionalButtons.Add(CreatorMenus.btnRotateRight);
            menu.InstructionalButtons.Add(CreatorMenus.btnRandom);
        }

        private async Task OnPlayerControls()
        {
            CreatorMenus._MenuPool.ProcessMouse();

            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Cache.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Cache.PlayerPed.Heading -= 10f;
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
