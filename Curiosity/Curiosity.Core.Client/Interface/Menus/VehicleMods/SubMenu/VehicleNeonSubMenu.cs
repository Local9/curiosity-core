using CitizenFX.Core;
using NativeUI;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu
{
    class VehicleNeonSubMenu
    {
        UIMenu menu;

        private UIMenuCheckboxItem uiChkUnderglowFront = new UIMenuCheckboxItem("Enable Front Light", false);
        private UIMenuCheckboxItem uiChkUnderglowBack = new UIMenuCheckboxItem("Enable Back Light", false);
        private UIMenuCheckboxItem uiChkUnderglowLeft = new UIMenuCheckboxItem("Enable Left Light", false);
        private UIMenuCheckboxItem uiChkUnderglowRight = new UIMenuCheckboxItem("Enable Right Light", false);
        private UIMenuListItem uiLstUnderglowColor;

        internal void Create(UIMenu neonMenu)
        {
            menu = neonMenu;

            var underglowColorsList = new List<dynamic>();
            for (int i = 0; i < 13; i++)
            {
                underglowColorsList.Add(GetLabelText($"CMOD_NEONCOL_{i}"));
            }

            uiLstUnderglowColor = new UIMenuListItem(GetLabelText("CMOD_NEON_1"), underglowColorsList, 0);

            menu.AddItem(uiLstUnderglowColor);
            menu.AddItem(uiChkUnderglowFront);
            menu.AddItem(uiChkUnderglowBack);
            menu.AddItem(uiChkUnderglowLeft);
            menu.AddItem(uiChkUnderglowRight);

            menu.OnCheckboxChange += Menu_OnCheckboxChange;
            menu.OnListChange += Menu_OnListChange;
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;
        }

        private void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle veh = Game.PlayerPed.CurrentVehicle;
                if (veh.Mods.HasNeonLights)
                {
                    veh.Mods.NeonLightsColor = GetColorFromIndex(uiLstUnderglowColor.Index);
                    if (checkboxItem == uiChkUnderglowLeft)
                    {
                        veh.Mods.SetNeonLightsOn(VehicleNeonLight.Left, veh.Mods.HasNeonLight(VehicleNeonLight.Left) && Checked);
                    }
                    else if (checkboxItem == uiChkUnderglowRight)
                    {
                        veh.Mods.SetNeonLightsOn(VehicleNeonLight.Right, veh.Mods.HasNeonLight(VehicleNeonLight.Right) && Checked);
                    }
                    else if (checkboxItem == uiChkUnderglowBack)
                    {
                        veh.Mods.SetNeonLightsOn(VehicleNeonLight.Back, veh.Mods.HasNeonLight(VehicleNeonLight.Back) && Checked);
                    }
                    else if (checkboxItem == uiChkUnderglowFront)
                    {
                        veh.Mods.SetNeonLightsOn(VehicleNeonLight.Front, veh.Mods.HasNeonLight(VehicleNeonLight.Front) && Checked);
                    }
                }
            }
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == uiLstUnderglowColor)
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    Vehicle veh = Game.PlayerPed.CurrentVehicle;
                    if (veh.Mods.HasNeonLights)
                    {
                        veh.Mods.NeonLightsColor = GetColorFromIndex(newIndex);
                    }
                }
            }
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward)
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                if (vehicle.Mods.HasNeonLights)
                {
                    uiChkUnderglowBack.Checked = vehicle.Mods.HasNeonLight(VehicleNeonLight.Back) && vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Back);
                    uiChkUnderglowFront.Checked = vehicle.Mods.HasNeonLight(VehicleNeonLight.Front) && vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Front);
                    uiChkUnderglowLeft.Checked = vehicle.Mods.HasNeonLight(VehicleNeonLight.Left) && vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Left);
                    uiChkUnderglowRight.Checked = vehicle.Mods.HasNeonLight(VehicleNeonLight.Right) && vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Right);

                    uiChkUnderglowBack.Enabled = true;
                    uiChkUnderglowFront.Enabled = true;
                    uiChkUnderglowLeft.Enabled = true;
                    uiChkUnderglowRight.Enabled = true;

                    uiChkUnderglowRight.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    uiChkUnderglowLeft.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    uiChkUnderglowBack.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    uiChkUnderglowFront.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                }
                else
                {
                    uiChkUnderglowBack.Enabled = false;
                    uiChkUnderglowFront.Enabled = false;
                    uiChkUnderglowLeft.Enabled = false;
                    uiChkUnderglowRight.Enabled = false;

                    uiChkUnderglowBack.Checked = false;
                    uiChkUnderglowFront.Checked = false;
                    uiChkUnderglowLeft.Checked = false;
                    uiChkUnderglowRight.Checked = false;

                    uiChkUnderglowRight.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    uiChkUnderglowLeft.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    uiChkUnderglowBack.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    uiChkUnderglowFront.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                }
            }
        }

        private readonly List<int[]> _VehicleNeonLightColors = new List<int[]>()
        {
            { new int[3] { 255, 255, 255 } },   // White
            { new int[3] { 2, 21, 255 } },      // Blue
            { new int[3] { 3, 83, 255 } },      // Electric blue
            { new int[3] { 0, 255, 140 } },     // Mint Green
            { new int[3] { 94, 255, 1 } },      // Lime Green
            { new int[3] { 255, 255, 0 } },     // Yellow
            { new int[3] { 255, 150, 5 } },     // Golden Shower
            { new int[3] { 255, 62, 0 } },      // Orange
            { new int[3] { 255, 0, 0 } },       // Red
            { new int[3] { 255, 50, 100 } },    // Pony Pink
            { new int[3] { 255, 5, 190 } },     // Hot Pink
            { new int[3] { 35, 1, 255 } },      // Purple
            { new int[3] { 15, 3, 255 } },      // Blacklight
        };

        private System.Drawing.Color GetColorFromIndex(int index)
        {
            if (index >= 0 && index < 13)
            {
                return System.Drawing.Color.FromArgb(_VehicleNeonLightColors[index][0], _VehicleNeonLightColors[index][1], _VehicleNeonLightColors[index][2]);
            }
            return System.Drawing.Color.FromArgb(255, 255, 255);
        }

        private int GetIndexFromColor()
        {
            Vehicle veh = Game.PlayerPed.CurrentVehicle;

            if (veh == null || !veh.Exists() || !veh.Mods.HasNeonLights)
            {
                return 0;
            }

            int r = 255, g = 255, b = 255;

            GetVehicleNeonLightsColour(veh.Handle, ref r, ref g, ref b);

            if (r == 255 && g == 0 && b == 255) // default return value when the vehicle has no neon kit selected.
            {
                return 0;
            }

            if (_VehicleNeonLightColors.Any(a => { return a[0] == r && a[1] == g && a[2] == b; }))
            {
                return _VehicleNeonLightColors.FindIndex(a => { return a[0] == r && a[1] == g && a[2] == b; });
            }

            return 0;
        }
    }
}
