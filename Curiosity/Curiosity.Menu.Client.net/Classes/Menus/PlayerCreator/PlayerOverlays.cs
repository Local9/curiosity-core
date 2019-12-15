using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenuAPI;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Menus.Client.net.Classes.Menus.PlayerCreator
{
    class PedHeadOverlay
    {
        public string OverlayType;
        public int ID;
        public string Name;
        public int maxOption;
        public int colorType = 0;
    }

    class PlayerOverlays
    {
        static bool HasSetupMenu = false;
        static int currentColor = 0;
        static Menu menu = new Menu("Makeup", "Customise your characters face");

        static List<PedHeadOverlay> overlays = new List<PedHeadOverlay>()
        {
            new PedHeadOverlay { ID = 0, Name = "Blemishes", maxOption = 23 },
            new PedHeadOverlay { ID = 1, Name = "Facial Hair", maxOption = 28, colorType = 1 },
            new PedHeadOverlay { ID = 2, Name = "Eyebrows", maxOption = 33, colorType = 1 },
            new PedHeadOverlay { ID = 3, Name = "Ageing", maxOption = 14 },
            new PedHeadOverlay { ID = 4, Name = "Makeup", maxOption = 74 },
            new PedHeadOverlay { ID = 5, Name = "Blush", maxOption = 6, colorType = 2 },
            new PedHeadOverlay { ID = 6, Name = "Complexion", maxOption = 11 },
            new PedHeadOverlay { ID = 7, Name = "Sun Damage", maxOption = 10 },
            new PedHeadOverlay { ID = 8, Name = "Lipstick", maxOption = 9, colorType = 2 },
            new PedHeadOverlay { ID = 9, Name = "Moles/Freckles", maxOption = 17 },
            new PedHeadOverlay { ID = 10, Name = "Chest Hair", maxOption = 16, colorType = 1 },
            new PedHeadOverlay { ID = 11, Name = "Body Blemishes", maxOption = 11 },
            new PedHeadOverlay { ID = 12, Name = "Birth Marks", maxOption = 1 }
        };
        static Dictionary<int, int> overlayColors = new Dictionary<int, int>();

        static List<string> GenerateNumberList(string txt, int max)
        {
            List<string> lst = new List<string>();
            for (int i = 0; i < max + 1; i++)
                lst.Add($"{txt} #{i.ToString()}");
            return lst;
        }

        public static async void Init()
        {
            while (!PlayerCreatorMenu.MenuSetup)
                await BaseScript.Delay(0);

            menu.OnMenuOpen += (_menu) =>
            {
                MenuBase.MenuOpen(true);

                if (HasSetupMenu)
                    return;

                overlays.ForEach(o =>
                {
                    menu.AddMenuItem(new MenuListItem(o.Name, GenerateNumberList("", o.maxOption), 0) { ItemData = new PedHeadOverlay { OverlayType = "OVERLAY_TYPE", ID = o.ID } });
                    if (o.colorType > 0)
                    {
                        menu.AddMenuItem(new MenuListItem(o.Name, GenerateNumberList("Color", o.maxOption), 0) { ItemData = new PedHeadOverlay { OverlayType = "OVERLAY_TYPE_COLOR", ID = o.ID, colorType = o.colorType } });
                    };
                });

                HasSetupMenu = true;
            };

            menu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
            };

            menu.OnListIndexChange += async (Menu _menu, MenuListItem _listItem, int _oldSelectionIndex, int _newSelectionIndex, int _itemIndex) =>
            {
                if (_listItem.ItemData.OverlayType == "OVERLAY_TYPE")
                {

                    if (_newSelectionIndex == 0)
                        currentColor = 0;

                    API.SetPedHeadOverlay(Client.PedHandle, _listItem.ItemData.ID, _newSelectionIndex, _newSelectionIndex == 0 ? 0f : 1f);
                    await BaseScript.Delay(0);
                    API.SetPedHeadOverlayColor(Client.PedHandle, _listItem.ItemData.ID, _listItem.ItemData.colorType, currentColor, currentColor);
                    await BaseScript.Delay(0);
                    PlayerCreatorMenu.StoreOverlay(_listItem.ItemData.ID, _newSelectionIndex);
                    await BaseScript.Delay(0);
                    PlayerCreatorMenu.StoreOverlayColor(_listItem.ItemData.ID, _listItem.ItemData.colorType, currentColor);
                    return;
                }

                if (_listItem.ItemData.OverlayType == "OVERLAY_TYPE_COLOR")
                {
                    currentColor = _newSelectionIndex;
                    PlayerCreatorMenu.StoreOverlayColor(_listItem.ItemData.ID, _listItem.ItemData.colorType, currentColor);
                    Function.Call(Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.PedHandle, _listItem.ItemData.ID, _listItem.ItemData.colorType, _newSelectionIndex, _newSelectionIndex);
                    return;
                }
            };

            MenuBase.AddSubMenu(PlayerCreatorMenu.menu, menu);
        }
    }
}
