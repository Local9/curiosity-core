using Curiosity.Core.Client.Interface.Menus.Data;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu
{
    class VehicleColorSubMenu
    {
        UIMenu baseMenu;
        UIMenu menuPrimary;
        int primaryIndex = 0;
        UIMenu menuSecondary;
        int secondaryIndex = 0;

        UIMenuListItem uiLstWheelColor;
        UIMenuListItem uiLstDashColor;
        UIMenuListItem uiLstTrimColor;
        UIMenuListItem uiLstChameleon;
        UIMenuSliderItem uiSldEnveff = new UIMenuSliderItem("Enveff Scale", "This works on certain vehicles.", false);
        UIMenuItem uiItmChrome = new UIMenuItem("Chrome");

        List<dynamic> classic = new List<dynamic>();
        List<dynamic> matte = new List<dynamic>();
        List<dynamic> metals = new List<dynamic>();
        List<dynamic> util = new List<dynamic>();
        List<dynamic> worn = new List<dynamic>();
        List<dynamic> chameleon = new List<dynamic>();
        List<dynamic> wheelColors = new List<dynamic>() { "Default Alloy" };

        int originalPrimaryIndex;
        int originalSecondaryIndex;

        internal void Create(UIMenu menu)
        {
            baseMenu = menu;
            baseMenu.MouseControlsEnabled = false;

            menuPrimary = VehicleModMenu._MenuPool.AddSubMenu(menu, "Primary Colors");
            menuSecondary = VehicleModMenu._MenuPool.AddSubMenu(menu, "Secondary Colors");

            uiSldEnveff.Maximum = 20;

            {
                int i = 0;
                foreach (var vc in VehicleData.ClassicColors)
                {
                    classic.Add($"{Game.GetGXTEntry(vc.label)} ({i + 1}/{VehicleData.ClassicColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MatteColors)
                {
                    matte.Add($"{Game.GetGXTEntry(vc.label)} ({i + 1}/{VehicleData.MatteColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MetalColors)
                {
                    metals.Add($"{Game.GetGXTEntry(vc.label)} ({i + 1}/{VehicleData.MetalColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.UtilColors)
                {
                    util.Add($"{Game.GetGXTEntry(vc.label)} ({i + 1}/{VehicleData.UtilColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.WornColors)
                {
                    worn.Add($"{Game.GetGXTEntry(vc.label)} ({i + 1}/{VehicleData.WornColors.Count})");
                    i++;
                }

                i = 0;
                chameleon.Add($"Remove {0}/{VehicleData.ChameleonPaint.Count}");
                foreach (var vc in VehicleData.ChameleonPaint)
                {
                    chameleon.Add($"{Game.GetGXTEntry(vc.label)} ({i + 1}/{VehicleData.ChameleonPaint.Count})");
                    i++;
                }

                wheelColors.AddRange(classic);
            }

            uiLstChameleon = new UIMenuListItem("Chameleon", chameleon, 0);
            uiLstDashColor = new UIMenuListItem("Dash Color", classic, 0);
            uiLstTrimColor = new UIMenuListItem("Trim Color", classic, 0);
            uiLstWheelColor = new UIMenuListItem("Wheel Color", wheelColors, 0);

            menu.AddItem(uiSldEnveff);
            menu.AddItem(uiLstChameleon);
            menu.AddItem(uiItmChrome);
            menu.AddItem(uiLstDashColor);
            menu.AddItem(uiLstTrimColor);
            menu.AddItem(uiLstWheelColor);

            for (int i = 0; i < 2; i++)
            {
                var pearlescentList = new UIMenuListItem("Pearlescent", classic, 0);
                var classicList = new UIMenuListItem("Classic", classic, 0);
                var metallicList = new UIMenuListItem("Metallic", classic, 0);
                var matteList = new UIMenuListItem("Matte", matte, 0);
                var metalList = new UIMenuListItem("Metals", metals, 0);
                var utilList = new UIMenuListItem("Util", util, 0);
                var utilChameleon = new UIMenuListItem("Chameleon", chameleon, 0);
                var wornList = new UIMenuListItem("Worn", worn, 0);

                if (i == 0)
                {
                    menuPrimary.AddItem(classicList);
                    menuPrimary.AddItem(metallicList);
                    menuPrimary.AddItem(matteList);
                    menuPrimary.AddItem(metalList);
                    menuPrimary.AddItem(utilList);
                    menuPrimary.AddItem(wornList);

                    menuPrimary.OnListChange += Menu_OnListChange;
                }
                else
                {
                    menuSecondary.AddItem(pearlescentList);
                    menuSecondary.AddItem(classicList);
                    menuSecondary.AddItem(metallicList);
                    menuSecondary.AddItem(matteList);
                    menuSecondary.AddItem(metalList);
                    menuSecondary.AddItem(utilList);
                    menuSecondary.AddItem(wornList);

                    menuSecondary.OnListChange += Menu_OnListChange;
                }
            }

            menu.OnItemSelect += (sender, item, index) =>
            {
                Vehicle veh = Game.PlayerPed.CurrentVehicle;
                if (item == uiItmChrome)
                {
                    int primaryColor = 0;
                    int secondaryColor = 0;

                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);

                    if (primaryColor == 120 && primaryColor == 120)
                    {
                        SetVehicleColours(veh.Handle, originalPrimaryIndex, originalSecondaryIndex);
                        return;
                    }

                    originalPrimaryIndex = primaryColor;
                    originalPrimaryIndex = secondaryColor;

                    SetVehicleColours(veh.Handle, 120, 120);
                }
            };

            menu.OnSliderChange += (sender, item, index) =>
            {
                Vehicle veh = Game.PlayerPed.CurrentVehicle;
                if (item == uiSldEnveff)
                {
                    SetVehicleEnveffScale(veh.Handle, index / 20f);
                }
            };

            menu.OnListChange += Menu_OnListChange;
            menuPrimary.OnIndexChange += Menu_OnIndexChange;
            menuSecondary.OnIndexChange += Menu_OnIndexChange;
        }

        private void Menu_OnIndexChange(UIMenu sender, int newIndex)
        {
            if (sender == menuPrimary)
                primaryIndex = newIndex;

            if (sender == menuSecondary)
                secondaryIndex = newIndex;
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            Vehicle veh = Game.PlayerPed.CurrentVehicle;

            int primaryColor = 0;
            int secondaryColor = 0;
            int pearlColor = 0;
            int wheelColor = 0;
            int dashColor = 0;
            int intColor = 0;

            GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
            GetVehicleExtraColours(veh.Handle, ref pearlColor, ref wheelColor);
            GetVehicleDashboardColour(veh.Handle, ref dashColor);
            GetVehicleInteriorColour(veh.Handle, ref intColor);

            if (sender == menuPrimary)
            {
                if (primaryIndex == 1)
                    pearlColor = VehicleData.ClassicColors[newIndex].id;
                else
                    pearlColor = 0;

                switch (primaryIndex)
                {
                    case 0:
                    case 1:
                        primaryColor = VehicleData.ClassicColors[newIndex].id;
                        break;
                    case 2:
                        primaryColor = VehicleData.MatteColors[newIndex].id;
                        break;
                    case 3:
                        primaryColor = VehicleData.MetalColors[newIndex].id;
                        break;
                    case 4:
                        primaryColor = VehicleData.UtilColors[newIndex].id;
                        break;
                    case 5:
                        primaryColor = VehicleData.WornColors[newIndex].id;
                        break;
                }
                SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
            }
            else if (sender == menuSecondary)
            {
                switch (secondaryIndex)
                {
                    case 0:
                        pearlColor = VehicleData.ClassicColors[newIndex].id;
                        break;
                    case 1:
                    case 2:
                        secondaryColor = VehicleData.ClassicColors[newIndex].id;
                        break;
                    case 3:
                        secondaryColor = VehicleData.MatteColors[newIndex].id;
                        break;
                    case 4:
                        secondaryColor = VehicleData.MetalColors[newIndex].id;
                        break;
                    case 5:
                        secondaryColor = VehicleData.UtilColors[newIndex].id;
                        break;
                    case 6:
                        secondaryColor = VehicleData.WornColors[newIndex].id;
                        break;
                }
                SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
            }
            else if (sender == baseMenu)
            {
                if (listItem == uiLstWheelColor)
                {
                    if (newIndex == 0)
                    {
                        wheelColor = 156; // default alloy color.
                    }
                    else
                    {
                        wheelColor = VehicleData.ClassicColors[newIndex - 1].id;
                    }
                }
                else if (listItem == uiLstDashColor)
                {
                    dashColor = VehicleData.ClassicColors[newIndex].id;
                    // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                    // this should actually be called SetVehicleDashboardColour
                    SetVehicleInteriorColour(veh.Handle, dashColor);
                }
                else if (listItem == uiLstTrimColor)
                {
                    intColor = VehicleData.ClassicColors[newIndex].id;
                    // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                    // this should actually be called SetVehicleInteriorColour
                    SetVehicleDashboardColour(veh.Handle, intColor);
                }
                else if (listItem == uiLstChameleon)
                {
                    int idx = newIndex - 1;

                    if (idx >= 0)
                    {
                        VehicleData.VehicleColor vehicleColor = VehicleData.ChameleonPaint[newIndex - 1];
                        SetVehicleColours(veh.Handle, vehicleColor.id, vehicleColor.id);
                        Logger.Debug($"Setting Vehicle Chameleon Paint: {vehicleColor.id}: {vehicleColor.label}");
                    }

                    if (idx == -1)
                    {
                        SetVehicleColours(veh.Handle, 0, 0);
                    }
                }
            }

            SetVehicleExtraColours(veh.Handle, pearlColor, wheelColor);
        }
    }
}
