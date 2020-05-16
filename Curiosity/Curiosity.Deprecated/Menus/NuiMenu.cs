using System;
using System.Collections.Generic;

namespace Curiosity.Client.net.Classes.Menus
{
    class NuiMenu
    {
        public static MenuObserver Observer;
        public static MenuModel NuiMenuModel;
        public static List<Tuple<int, MenuItem, Func<bool>>> ItemsAll = new List<Tuple<int, MenuItem, Func<bool>>>();
        internal static List<MenuItem> ItemsFiltered = new List<MenuItem>();
        public static bool IsDirty = false;
        static bool IsInventoryDisabled = false;

        class NuiMenuOptions : MenuModel
        {
            public override void Refresh()
            {
                var _menuItems = new List<MenuItem>();

                if (!IsInventoryDisabled)
                {
                    MenuItem inventoryPanel = new MenuItemStandard
                    {
                        Title = "Inventory",
                        Description = "Character Inventory",
                        OnActivate = new Action<MenuItemStandard>(async (events) =>
                        {
                            await BaseScript.Delay(100);
                            BaseScript.TriggerServerEvent("curiosity:Server:Inventory:GetItems");
                            // Observer.CloseMenu(true);
                        })
                    };

                    _menuItems.Add(inventoryPanel);
                }

                _menuItems.Add(new MenuItemStandard { Title = "Team Management", Description = "Work in Progress" });
                _menuItems.Add(new MenuItemStandard { Title = "Clan Management", Description = "Work in Progress" });

                menuItems = _menuItems;
            }
        }

        public static void Init()
        {
            try
            {
                MenuOptions PedMenuOptions = new MenuOptions { Origin = new PointF(700, 200) };
                NuiMenuModel = new NuiMenuOptions { numVisibleItems = 7 };
                NuiMenuModel.headerTitle = "Panels";
                NuiMenuModel.statusTitle = "";
                NuiMenuModel.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Populating..." } };

                InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu
                {
                    Title = $"Panels",
                    SubMenu = NuiMenuModel
                }, () => true);

                Client.GetInstance().RegisterEventHandler("curiosity:Client:Settings:Inventory", new Action<bool>(InventorySettings));
                PeriodicCheck();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"{ex.InnerException}");

            }
        }

        static async void PeriodicCheck()
        {
            await BaseScript.Delay(3000);
            BaseScript.TriggerServerEvent("curiosity:Server:Settings:Inventory");
            while (true)
            {
                await BaseScript.Delay(60000);
                BaseScript.TriggerServerEvent("curiosity:Server:Settings:Inventory");
            }
        }

        static void InventorySettings(bool setting)
        {
            IsInventoryDisabled = setting;
        }
    }
}
