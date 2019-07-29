//using CitizenFX.Core;
//using CitizenFX.Core.Native;
//using Curiosity.Shared.Client.net;
//using MenuAPI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Curiosity.Shared.Client.net.Extensions;

//namespace Curiosity.Menus.Client.net.Classes.Menus.PlayerCreator
//{
//    internal class PropType
//    {
//        public int Type;
//        public PedProp PedProp;
//    }

//    class PlayerProps
//    {
//        static List<PedProps> PedPropValues = Enum.GetValues(typeof(PedProps)).OfType<PedProps>().ToList();
//        static List<string> PedPropNames = Enum.GetNames(typeof(PedProps)).Select(c => c.AddSpacesToCamelCase()).ToList();
//        static bool HasSetupMenu = false;
//        static Menu menu = new Menu("Accessories", "Accessories");
//        static Dictionary<string, Tuple<int, int>> propSettings = new Dictionary<string, Tuple<int, int>>();
//        static List<string> currentVariants = PlayerCreatorMenu.GenerateNumberList("", 1);
//        static MenuListItem menuVariants;

//        static Dictionary<string, string> componentAndPropRenamings = new Dictionary<string, string>()
//        {
//            ["Torso"] = "Arms",
//            ["Legs"] = "Pants",
//            ["Hands"] = "Parachutes, Vests and Bags",
//            ["Special1"] = "Neck",
//            ["Special2"] = "Overshirt",
//            ["Special3"] = "Tactical Vests",
//            ["Textures"] = "Logos",
//            ["Torso2"] = "Jacket",
//            ["EarPieces"] = "Ear Pieces"
//        };

//        public static async void Init()
//        {
//            while (!PlayerCreatorMenu.MenuSetup)
//                await BaseScript.Delay(0);

//            menu.OnMenuOpen += (_menu) =>
//            {
//                MenuBase.MenuOpen(true);
//                if (HasSetupMenu)
//                    return;

//                PedProp[] props = Game.PlayerPed.Style.GetAllProps();

//                props.ToList().ForEach(p => {
//                    try
//                    {
//                        if (!propSettings.ContainsKey(p.ToString())) propSettings.Add(p.ToString(), new Tuple<int, int>(0, 0));
//                        if (propSettings[p.ToString()].Item1 < -1 || propSettings[p.ToString()].Item1 > p.Count - 1) propSettings[p.ToString()] = new Tuple<int, int>(0, propSettings[p.ToString()].Item2);
//                        if (propSettings[p.ToString()].Item2 < 0 || propSettings[p.ToString()].Item2 > p.TextureCount - 1) propSettings[p.ToString()] = new Tuple<int, int>(propSettings[p.ToString()].Item1, 0);

//                        if (p.HasVariations)
//                        {
//                            int index = 0;
//                            if (Client.User.Skin.Props.ContainsKey(Enum.GetNames(typeof(PedProps)).ToList().IndexOf(p.ToString())))
//                                index = Client.User.Skin.Props[Enum.GetNames(typeof(PedProps)).ToList().IndexOf(p.ToString())].Item1;

//                            menu.AddMenuItem(new MenuListItem($@"{(componentAndPropRenamings.ContainsKey(p.ToString()) ? componentAndPropRenamings[p.ToString()] : p.ToString())}", PlayerCreatorMenu.GenerateNumberList("", p.Count - 1), 0) { ItemData = new PropType() { Type = 1, PedProp = p } });
//                        }

//                        int indexVariant = 0;
//                        if (Client.User.Skin.Props.ContainsKey(Enum.GetNames(typeof(PedProps)).ToList().IndexOf(p.ToString())))
//                            indexVariant = Client.User.Skin.Props[Enum.GetNames(typeof(PedProps)).ToList().IndexOf(p.ToString())].Item2;

//                        menu.AddMenuItem(new MenuListItem($@"{(componentAndPropRenamings.ContainsKey(p.ToString()) ? componentAndPropRenamings[p.ToString()] : p.ToString())}: Variants", currentVariants, indexVariant) { ItemData = new PropType() { Type = 2, PedProp = p }, Enabled = indexVariant > 0 });
//                    }
//                    catch (Exception ex)
//                    {
//                        Log.Error($"[PlayerProps] Exception in components code; {ex.Message}");
//                    }
//                });

//                HasSetupMenu = true;
//            };


//            menu.OnMenuClose += (_menu) =>
//            {
//                MenuBase.MenuOpen(false);
//            };

//            int currentProp = 0;
//            int currentTexture = 0;

//            menu.OnListIndexChange += async (Menu _menu, MenuListItem _listItem, int _oldSelectionIndex, int _newSelectionIndex, int _itemIndex) =>
//            {
//                await BaseScript.Delay(0);
//                if (_listItem.ItemData.Type == 2)
//                {
//                    propSettings[_listItem.ItemData.PedProp.ToString()] = new Tuple<int, int>(propSettings[_listItem.ItemData.PedProp.ToString()].Item1, _newSelectionIndex);
//                    Function.Call(Hash.SET_PED_PROP_INDEX, Client.PedHandle, Enum.GetNames(typeof(PedProps)).ToList().IndexOf(_listItem.ItemData.PedProp.ToString()), currentProp, _newSelectionIndex, false);
//                    currentTexture = _newSelectionIndex;
//                }
//                else
//                {
//                    propSettings[_listItem.ItemData.PedProp.ToString()] = new Tuple<int, int>(_newSelectionIndex, 0);
//                    if (_newSelectionIndex == 0)
//                    {
//                        Function.Call(Hash.CLEAR_PED_PROP, Client.PedHandle, Enum.GetNames(typeof(PedProps)).ToList().IndexOf(_listItem.ItemData.PedProp.ToString()));
//                    }
//                    else
//                    {
//                        Function.Call(Hash.SET_PED_PROP_INDEX, Client.PedHandle, Enum.GetNames(typeof(PedProps)).ToList().IndexOf(_listItem.ItemData.PedProp.ToString()), _newSelectionIndex, 0, false);
//                    }
//                    currentProp = _newSelectionIndex;

//                    MenuListItem m = (MenuListItem)_menu.GetMenuItems()[_itemIndex + 1];

//                    if (_listItem.ItemData.PedProp.HasTextureVariations)
//                    {
//                        m.Enabled = true;
//                        m.ListItems = PlayerCreatorMenu.GenerateNumberList("", _listItem.ItemData.PedProp.TextureCount - 1);
//                        m.ListIndex = 0;
//                    }
//                    else
//                    {
//                        m.Enabled = false;
//                        m.ListItems = PlayerCreatorMenu.GenerateNumberList("", 0);
//                        m.ListIndex = 0;
//                    }
//                }

//                PlayerCreatorMenu.StoreProps(Enum.GetNames(typeof(PedProps)).ToList().IndexOf(_listItem.ItemData.PedProp.ToString()), currentProp, currentTexture);
//            };

//            MenuBase.AddSubMenu(PlayerCreatorMenu.menu, menu);
//        }
//    }
//}
