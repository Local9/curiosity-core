using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net;
using MenuAPI;

namespace Curiosity.Client.net.Classes.Menus.PlayerCreator
{
    class PlayerComponents
    {
        static Menu menu = new Menu("Player Components", "Player components");

        static Dictionary<string, Tuple<int, int>> componentSettings = new Dictionary<string, Tuple<int, int>>();
        static Dictionary<string, Tuple<int, int>> propSettings = new Dictionary<string, Tuple<int, int>>();

        static Dictionary<string, string> componentAndPropRenamings = new Dictionary<string, string>()
        {
            ["Torso"] = "Arms",
            ["Legs"] = "Pants",
            ["Hands"] = "Parachutes, Vests and Bags",
            ["Special1"] = "Neck",
            ["Special2"] = "Overshirt",
            ["Special3"] = "Tactical Vests",
            ["Textures"] = "Logos",
            ["Torso"] = "Arms",
            ["Torso2"] = "Jacket",
            ["EarPieces"] = "Ear Pieces"
        };

        static List<PedProps> PedPropValues = Enum.GetValues(typeof(PedProps)).OfType<PedProps>().ToList();
        static List<string> PedPropNames = Enum.GetNames(typeof(PedProps)).Select(c => c.AddSpacesToCamelCase()).ToList();

        public static void Init()
        {
            PedComponent[] components = Game.PlayerPed.Style.GetAllComponents();
            PedProp[] props = Game.PlayerPed.Style.GetAllProps();

            components.ToList().ForEach(c =>
            {
                try
                {
                    if (!(c.ToString() == "Face") || (!((PedHash)Game.PlayerPed.Model.Hash == PedHash.FreemodeMale01) && !((PedHash)Game.PlayerPed.Model.Hash == PedHash.FreemodeFemale01))) // Since Face doesn't work on freemode characters (if you use the blending/morphing option anyway, which everybody should be)
                    {
                        if (!componentSettings.ContainsKey(c.ToString())) componentSettings.Add(c.ToString(), new Tuple<int, int>(0, 0));
                        if (componentSettings[c.ToString()].Item1 < 0 || componentSettings[c.ToString()].Item1 > c.Count - 1) componentSettings[c.ToString()] = new Tuple<int, int>(0, componentSettings[c.ToString()].Item2);
                        if (componentSettings[c.ToString()].Item2 < 0 || componentSettings[c.ToString()].Item2 > c.TextureCount - 1) componentSettings[c.ToString()] = new Tuple<int, int>(componentSettings[c.ToString()].Item1, 0);
                        if (c.Count > 1)
                        {
                            menu.AddMenuItem(new MenuListItem($@"{(componentAndPropRenamings.ContainsKey(c.ToString()) ? componentAndPropRenamings[c.ToString()] : c.ToString())}", PlayerCreatorMenu.GenerateNumberList("", c.Count - 1), 0) { ItemData = c });
                        }
                        if (c.TextureCount > 1)
                        {
                            menu.AddMenuItem(new MenuListItem($@"{(componentAndPropRenamings.ContainsKey(c.ToString()) ? componentAndPropRenamings[c.ToString()] : c.ToString())}: Variants", PlayerCreatorMenu.GenerateNumberList("", c.TextureCount - 1), 0) { ItemData = c });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[PlayerComponents] Exception in components code; {ex.Message}");
                }
            });

            menu.OnMenuOpen += (_menu) =>
            {
                Environment.UI.Location.HideLocation = true;
            };

            menu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
            };

            menu.OnListIndexChange += async (Menu _menu, MenuListItem _listItem, int _oldSelectionIndex, int _newSelectionIndex, int _itemIndex) =>
            {
                await BaseScript.Delay(0);
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldSelectionIndex}, {_newSelectionIndex}, {_itemIndex}, {_listItem.ItemData.ToString()}]");
                if (_listItem.ItemData.TextureCount > 1)
                {
                    componentSettings[_listItem.ItemData.ToString()] = new Tuple<int, int>(componentSettings[_listItem.ItemData.ToString()].Item1, _newSelectionIndex);
                    API.SetPedComponentVariation(Client.PedHandle, Enum.GetNames(typeof(PedComponents)).ToList().IndexOf(_listItem.ItemData.ToString()), componentSettings[_listItem.ItemData.ToString()].Item1, _newSelectionIndex, 0);
                }
                else
                {
                    componentSettings[_listItem.ItemData.ToString()] = new Tuple<int, int>(_newSelectionIndex, 0);
                    API.SetPedComponentVariation(Client.PedHandle, Enum.GetNames(typeof(PedComponents)).ToList().IndexOf(_listItem.ItemData.ToString()), _newSelectionIndex, 0, 0);
                }
            };

            MenuBase.AddSubMenu(PlayerCreatorMenu.menu, menu);
        }
    }
}
