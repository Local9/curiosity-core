using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Client.net.Classes.Menus.PlayerCreator
{
    internal class ComponentType
    {
        public int Type;
        public PedComponent PedComponent;
    }

    class PlayerComponents
    {
        static Menu menu = new Menu("Player Components", "Player components");

        static Dictionary<string, Tuple<int, int>> componentSettings = new Dictionary<string, Tuple<int, int>>();

        static Dictionary<string, string> componentAndPropRenamings = new Dictionary<string, string>()
        {
            ["Torso"] = "Arms",
            ["Legs"] = "Pants",
            ["Hands"] = "Parachutes, Vests and Bags",
            ["Special1"] = "Neck",
            ["Special2"] = "Overshirt",
            ["Special3"] = "Tactical Vests",
            ["Textures"] = "Logos",
            ["Torso2"] = "Jacket",
            ["EarPieces"] = "Ear Pieces"
        };

        public static void Init()
        {
            menu.OnMenuOpen += (_menu) =>
            {
                Environment.UI.Location.HideLocation = true;

                PedComponent[] components = Game.PlayerPed.Style.GetAllComponents();

                components.ToList().ForEach(c =>
                {
                    try
                    {
                        if (!(c.ToString() == "Face") || (!((PedHash)Game.PlayerPed.Model.Hash == PedHash.FreemodeMale01) && !((PedHash)Game.PlayerPed.Model.Hash == PedHash.FreemodeFemale01))) // Since Face doesn't work on freemode characters (if you use the blending/morphing option anyway, which everybody should be)
                        {
                            if (!componentSettings.ContainsKey(c.ToString())) componentSettings.Add(c.ToString(), new Tuple<int, int>(0, 0));
                            if (componentSettings[c.ToString()].Item1 < 0 || componentSettings[c.ToString()].Item1 > c.Count - 1) componentSettings[c.ToString()] = new Tuple<int, int>(0, componentSettings[c.ToString()].Item2);
                            if (componentSettings[c.ToString()].Item2 < 0 || componentSettings[c.ToString()].Item2 > c.TextureCount - 1) componentSettings[c.ToString()] = new Tuple<int, int>(componentSettings[c.ToString()].Item1, 0);
                            if (c.HasVariations)
                            {
                                menu.AddMenuItem(new MenuListItem($@"{(componentAndPropRenamings.ContainsKey(c.ToString()) ? componentAndPropRenamings[c.ToString()] : c.ToString())}", PlayerCreatorMenu.GenerateNumberList("", c.Count - 1), 0) { ItemData = new ComponentType() { Type = 1, PedComponent = c } });
                            }
                            if (c.HasTextureVariations && !(c.ToString() == "Hair"))
                            {
                                menu.AddMenuItem(new MenuListItem($@"{(componentAndPropRenamings.ContainsKey(c.ToString()) ? componentAndPropRenamings[c.ToString()] : c.ToString())}: Variants", PlayerCreatorMenu.GenerateNumberList("", c.TextureCount - 1), 0) { ItemData = new ComponentType() { Type = 2, PedComponent = c } });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[PlayerComponents] Exception in components code; {ex.Message}");
                    }
                });
            };

            menu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
            };

            int currentModel = 0;
            int currentTexture = 0;

            menu.OnListIndexChange += async (Menu _menu, MenuListItem _listItem, int _oldSelectionIndex, int _newSelectionIndex, int _itemIndex) =>
            {
                await BaseScript.Delay(0);
                if (_listItem.ItemData.Type == 2)
                {
                    currentTexture = _newSelectionIndex;
                    componentSettings[_listItem.ItemData.PedComponent.ToString()] = new Tuple<int, int>(currentModel, currentTexture);
                    API.SetPedComponentVariation(Client.PedHandle, Enum.GetNames(typeof(PedComponents)).ToList().IndexOf(_listItem.ItemData.PedComponent.ToString()), currentModel, currentTexture, 0);
                }
                else
                {
                    currentModel = _newSelectionIndex;
                    componentSettings[_listItem.ItemData.PedComponent.ToString()] = new Tuple<int, int>(currentModel, 0);
                    API.SetPedComponentVariation(Client.PedHandle, Enum.GetNames(typeof(PedComponents)).ToList().IndexOf(_listItem.ItemData.PedComponent.ToString()), currentModel, 0, 0);
                }

            };

            MenuBase.AddSubMenu(PlayerCreatorMenu.menu, menu);
        }
    }
}
