using CitizenFX.Core;
using Curiosity.Core.Client.Interface.Menus.Data;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu
{
    class VehicleColorSubMenu
    {
        UIMenu menuPrimary;
        UIMenu menuSecondary;

        UIMenuListItem uiLstWheelColor;
        UIMenuListItem uiLstDashColor;
        UIMenuListItem uiLstTrimColor;
        UIMenuSliderItem uiSldEnveff = new UIMenuSliderItem("Enveff Scale", "This works on certain vehicles.", false);
        UIMenuItem uiItmChrome = new UIMenuItem("Chrome");

        List<dynamic> classic = new List<dynamic>();
        List<dynamic> matte = new List<dynamic>();
        List<dynamic> metals = new List<dynamic>();
        List<dynamic> util = new List<dynamic>();
        List<dynamic> worn = new List<dynamic>();
        List<dynamic> wheelColors = new List<dynamic>() { "Default Alloy" };

        internal void Create(UIMenu menu)
        {
            uiSldEnveff.Maximum = 20;

            {
                int i = 0;
                foreach (var vc in VehicleData.ClassicColors)
                {
                    classic.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.ClassicColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MatteColors)
                {
                    matte.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MatteColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MetalColors)
                {
                    metals.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MetalColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.UtilColors)
                {
                    util.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.UtilColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.WornColors)
                {
                    worn.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.WornColors.Count})");
                    i++;
                }

                wheelColors.AddRange(classic);
            }

            uiLstDashColor = new UIMenuListItem("Dash Color", classic, 0);
            uiLstTrimColor = new UIMenuListItem("Trim Color", classic, 0);
            uiLstWheelColor = new UIMenuListItem("Wheel Color", wheelColors, 0);

            menu.AddItem(uiSldEnveff);
            menu.AddItem(uiItmChrome);
            menu.AddItem(uiLstDashColor);
            menu.AddItem(uiLstTrimColor);
            menu.AddItem(uiLstWheelColor);

            menu.OnItemSelect += (sender, item, index) =>
            {
                Vehicle veh = Game.PlayerPed.CurrentVehicle;
                if (item == uiItmChrome)
                {
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
        }
    }
}
