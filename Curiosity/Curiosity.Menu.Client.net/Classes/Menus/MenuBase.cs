﻿using MenuAPI;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class MenuBase
    {
        static Client client = Client.GetInstance();

        public static Menu Menu = new Menu("Interaction Menu", "Interaction Menu");
        public static bool isMenuOpen = Menu.Visible;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Player:Menu:VehicleId", new Action<int>(OnVehicleId));

            //// Setting the menu alignment to be right aligned. This can be changed at any time and it'll update instantly.
            //// To test this, checkout one of the checkbox items in this example menu. Clicking it will toggle the menu alignment.
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
            //// Creating the first menu.

            // menu.HeaderTexture = new KeyValuePair<string, string>("shopui_title_graphics_franklin", "shopui_title_graphics_franklin");
            

            //// Creating 3 sliders, showing off the 3 possible variations and custom colors.
            //MenuSliderItem slider = new MenuSliderItem("Slider", 0, 10, 5, false);
            //MenuSliderItem slider2 = new MenuSliderItem("Slider + Bar", 0, 10, 5, true)
            //{
            //    BarColor = Color.FromArgb(255, 73, 233, 111),
            //    BackgroundColor = Color.FromArgb(255, 25, 100, 43)
            //};
            //MenuSliderItem slider3 = new MenuSliderItem("Slider + Bar + Icons", "The icons are currently male/female because that's probably the most common use. But any icon can be used!", 0, 10, 5, true)
            //{
            //    BarColor = Color.FromArgb(255, 255, 0, 0),
            //    BackgroundColor = Color.FromArgb(255, 100, 0, 0),
            //    SliderLeftIcon = MenuItem.Icon.MALE,
            //    SliderRightIcon = MenuItem.Icon.FEMALE
            //};
            //// adding the sliders to the menu.
            //menu.AddMenuItem(slider);
            //menu.AddMenuItem(slider2);
            //menu.AddMenuItem(slider3);

            //// Creating 3 checkboxs, 2 different styles and one has a locked icon and it's 'not enabled' (not enabled meaning you can't toggle it).
            //MenuCheckboxItem box = new MenuCheckboxItem("Checkbox - Style 1 (click me!)", "This checkbox can toggle the menu position! Try it out.", !menu.LeftAligned)
            //{
            //    Style = MenuCheckboxItem.CheckboxStyle.Tick
            //};

            //MenuCheckboxItem box2 = new MenuCheckboxItem("Checkbox - Style 2", "This checkbox does nothing right now.", true)
            //{
            //    Style = MenuCheckboxItem.CheckboxStyle.Cross
            //};

            //MenuCheckboxItem box3 = new MenuCheckboxItem("Checkbox (unchecked + locked)", "Make this menu right aligned. If you set this to false, then the menu will move to the left.", false)
            //{
            //    Style = MenuCheckboxItem.CheckboxStyle.Cross,
            //    Enabled = false,
            //    LeftIcon = MenuItem.Icon.LOCK
            //};

            //// Adding the checkboxes to the menu.
            //menu.AddMenuItem(box);
            //menu.AddMenuItem(box2);
            //menu.AddMenuItem(box3);

            //// Dynamic list item
            //string ChangeCallback(MenuDynamicListItem item, bool left)
            //{
            //    if (left)
            //        return (int.Parse(item.CurrentItem) - 1).ToString();
            //    return (int.Parse(item.CurrentItem) + 1).ToString();
            //}
            //MenuDynamicListItem dynList = new MenuDynamicListItem("Dynamic list item.", "0", new MenuDynamicListItem.ChangeItemCallback(ChangeCallback), "Description for this dynamic item. Pressing left will make the value smaller, pressing right will make the value bigger.");
            //menu.AddMenuItem(dynList);

            //// List items (first the 3 special variants, then a normal one)
            //List<string> colorList = new List<string>();
            //for (var i = 0; i < 64; i++)
            //{
            //    colorList.Add($"Color #{i}");
            //}
            //MenuListItem hairColors = new MenuListItem("Hair Color", colorList, 0, "Hair color pallete.")
            //{
            //    ShowColorPanel = true
            //};

            //// Also special
            //List<string> makeupColorList = new List<string>();
            //for (var i = 0; i < 64; i++)
            //{
            //    makeupColorList.Add($"Color #{i}");
            //}
            //MenuListItem makeupColors = new MenuListItem("Makeup Color", makeupColorList, 0, "Makeup color pallete.")
            //{
            //    ShowColorPanel = true,
            //    ColorPanelColorType = MenuListItem.ColorPanelType.Makeup
            //};

            //// Also special
            //List<string> opacityList = new List<string>();
            //for (var i = 0; i < 11; i++)
            //{
            //    opacityList.Add($"Opacity {i * 10}%");
            //}
            //MenuListItem opacity = new MenuListItem("Opacity Panel", opacityList, 0, "Set an opacity for something.")
            //{
            //    ShowOpacityPanel = true
            //};

            //// Normal
            //List<string> quickGpsList = new List<string>() { "None", "Home", "Some other fckin place" };
            //MenuListItem quickGpsMenuListItem = new MenuListItem("Quick GPS", quickGpsList, 0, "Select to place your waypoint at a set location.");

            //// Adding the lists to the menu.
            //menu.AddMenuItem(hairColors);
            //menu.AddMenuItem(makeupColors);
            //menu.AddMenuItem(opacity);
            //menu.AddMenuItem(quickGpsMenuListItem);

            //// Creating a submenu, adding it to the menus list, and creating and binding a button for it.
            //Menu submenu = new Menu("Submenu", "Secondary Menu");
            //MenuController.AddSubmenu(menu, submenu);

            //MenuItem menuButton = new MenuItem("Submenu", "This button is bound to a submenu. Clicking it will take you to the submenu.")
            //{
            //    Label = "→→→"
            //};
            //menu.AddMenuItem(menuButton);
            //MenuController.BindMenuItem(menu, submenu, menuButton);

            //// Adding items with sprites left & right to the submenu.
            //for (var i = 0; i < 30; i++)
            //{
            //    var tmpItem = new MenuItem($"Icon Sprite #{i}", "This menu item has a left and right sprite, and some also have a right label! Very cool huh?!");
            //    if (i % 4 == 0)
            //    {
            //        tmpItem.Label = "Wowzers";
            //    }
            //    if (i % 7 == 0)
            //    {
            //        tmpItem.Label = "Snailsome!";
            //    }
            //    tmpItem.LeftIcon = (MenuItem.Icon)i;
            //    tmpItem.RightIcon = (MenuItem.Icon)i;

            //    submenu.AddMenuItem(tmpItem);
            //}

            //// Instructional buttons setup for the second (submenu) menu.
            //submenu.InstructionalButtons.Add(Control.CharacterWheel, "Right?!");
            //submenu.InstructionalButtons.Add(Control.CreatorLS, "Buttons");
            //submenu.InstructionalButtons.Add(Control.CursorScrollDown, "Cool");
            //submenu.InstructionalButtons.Add(Control.CreatorDelete, "Out!");
            //submenu.InstructionalButtons.Add(Control.Cover, "This");
            //submenu.InstructionalButtons.Add(Control.Context, "Check");

            //// Create a third menu without a banner.
            //Menu menu3 = new Menu(null, "Only a subtitle, no banner.");

            //// you can use AddSubmenu or AddMenu, both will work but if you want to link this menu from another menu,
            //// you should use AddSubmenu.
            //MenuController.AddSubmenu(menu, menu3);
            //MenuItem thirdSubmenuBtn = new MenuItem("Another submenu", "This is just a submenu without a banner. No big deal.") { Label = "→→→" };
            //menu.AddMenuItem(thirdSubmenuBtn);
            //MenuController.BindMenuItem(menu, menu3, thirdSubmenuBtn);
            //menu3.AddMenuItem(new MenuItem("Nothing here!"));
            //menu3.AddMenuItem(new MenuItem("Nothing here!"));
            //menu3.AddMenuItem(new MenuItem("Nothing here!"));
            //menu3.AddMenuItem(new MenuItem("Nothing here!"));


            ///*
            // ########################################################
            //                     Event handlers
            // ########################################################
            //*/


            //menu.OnCheckboxChange += (_menu, _item, _index, _checked) =>
            //{
            //    // Code in here gets executed whenever a checkbox is toggled.
            //    Debug.WriteLine($"OnCheckboxChange: [{_menu}, {_item}, {_index}, {_checked}]");

            //    // If the align-menu checkbox is toggled, toggle the menu alignment.
            //    if (_item == box)
            //    {
            //        if (_checked)
            //        {
            //            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
            //        }
            //        else
            //        {
            //            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
            //        }
            //    }
            //};

            Menu.OnItemSelect += (_menu, _item, _index) =>
            {

            };

            //menu.OnIndexChange += (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            //{
            //    // Code in here would get executed whenever the up or down key is pressed and the index of the menu is changed.
            //    Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
            //};

            //menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            //{
            //    // Code in here would get executed whenever the selected value of a list item changes (when left/right key is pressed).
            //    Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");
            //};

            Menu.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
            };

            //menu.OnSliderPositionChange += (_menu, _sliderItem, _oldPosition, _newPosition, _itemIndex) =>
            //{
            //    // Code in here would get executed whenever the position of a slider is changed (when left/right key is pressed).
            //    Debug.WriteLine($"OnSliderPositionChange: [{_menu}, {_sliderItem}, {_oldPosition}, {_newPosition}, {_itemIndex}]");
            //};

            //menu.OnSliderItemSelect += (_menu, _sliderItem, _sliderPosition, _itemIndex) =>
            //{
            //    // Code in here would get executed whenever a slider item is pressed.
            //    Debug.WriteLine($"OnSliderItemSelect: [{_menu}, {_sliderItem}, {_sliderPosition}, {_itemIndex}]");
            //};

            Menu.OnMenuClose += (_menu) =>
            {
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            };

            Menu.OnMenuOpen += (_menu) =>
            {
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            };

            //menu.OnDynamicListItemCurrentItemChange += (_menu, _dynamicListItem, _oldCurrentItem, _newCurrentItem) =>
            //{
            //    // Code in here would get executed whenever the value of the current item of a dynamic list item changes.
            //    Debug.WriteLine($"OnDynamicListItemCurrentItemChange: [{_menu}, {_dynamicListItem}, {_oldCurrentItem}, {_newCurrentItem}]");
            //};

            //menu.OnDynamicListItemSelect += (_menu, _dynamicListItem, _currentItem) =>
            //{
            //    // Code in here would get executed whenever a dynamic list item is pressed.
            //    Debug.WriteLine($"OnDynamicListItemSelect: [{_menu}, {_dynamicListItem}, {_currentItem}]");
            //};

            MenuController.AddMenu(Menu);

            //// Classes.Menus.Inventory.Init();
            Classes.Menus.PlayerMenu.Init();
            Classes.Menus.PlayerCreator.PlayerCreatorMenu.Init();
            Classes.Menus.PlayerCreator.PlayerOverlays.Init();
            Classes.Menus.PlayerCreator.PlayerComponents.Init();
            Classes.Menus.PlayerCreator.PlayerProps.Init();
            Classes.Menus.PlayerCreator.PlayerSave.Init();
            //// ONLINE PLAYER MENU ITEMS
            Classes.Menus.OnlinePlayers.Init();
            Classes.Menus.PlayerInteractions.ReportInteraction.Init();
            Classes.Menus.PlayerInteractions.KickInteraction.Init();
            Classes.Menus.PlayerInteractions.BanInteraction.Init();
            //// VEHICLE
            Classes.Menus.VehicleMenu.Init();
        }

        public static void MenuOpen(bool isOpen)
        {
            //if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
            //{
            //    Debug.WriteLine($"MenuOpen: {isOpen}");
            //}

            if (isOpen)
            {
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            }
            else
            {
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            }
        }

        public static void AddMenuItem(MenuItem menuItem)
        {
            Menu.AddMenuItem(menuItem);
        }

        public static void AddSubMenu(Menu submenu)
        {
            AddSubMenu(Menu, submenu);
        }

        public static void AddSubMenu(Menu menu, Menu submenu)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = "→→→" };
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
        }

        public static void RemoveMenu(Menu menu)
        {
            foreach(MenuItem menuItem in Menu.GetMenuItems())
            {
                if (menu.MenuTitle == menuItem.Text)
                    Menu.RemoveMenuItem(menuItem);
            }
        }

        public static void OnVehicleId(int vehicleId)
        {
            Client.CurrentVehicle = new Vehicle(vehicleId);
        }
    }
}
