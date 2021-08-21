using CitizenFX.Core;
using NativeUI;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu
{
    class VehicleLiveriesSubMenu
    {
        UIMenu baseMenu;
        UIMenuListItem liveryListItem;

        internal void Create(UIMenu menu)
        {
            baseMenu = menu;
            baseMenu.OnMenuStateChanged += Menu_OnMenuStateChanged;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (Equals(MenuState.Opened, state) || Equals(MenuState.ChangeForward, state))
            {
                UpdateMenu();
            }
        }

        private void UpdateMenu()
        {
            baseMenu.Clear();

            Vehicle veh = Game.PlayerPed.CurrentVehicle;

            SetVehicleModKit(veh.Handle, 0);
            var liveryCount = veh.Mods.LiveryCount;

            if (liveryCount > 0)
            {
                var liveryList = new List<dynamic>();
                for (var i = 0; i < liveryCount; i++)
                {
                    var livery = GetLiveryName(veh.Handle, i);
                    livery = GetLabelText(livery) != "NULL" ? GetLabelText(livery) : $"Livery #{i}";
                    liveryList.Add(livery);
                }

                liveryListItem = new UIMenuListItem("Set Livery", liveryList, GetVehicleLivery(veh.Handle), "Choose a livery for this vehicle.");
                baseMenu.AddItem(liveryListItem);

                baseMenu.OnListChange += (_menu, listItem, newIndex) =>
                {
                    if (listItem == liveryListItem)
                    {
                        veh = Game.PlayerPed.CurrentVehicle;
                        veh.Mods.Livery = newIndex;
                    }
                };
                baseMenu.RefreshIndex();
            }
            else
            {
                Notify.Error("This vehicle does not have any liveries.");
            }
        }
    }
}
