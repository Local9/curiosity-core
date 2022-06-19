using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using NativeUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    class CharacterAppearance
    {
        private bool FaceCameraActive = false;
        private UIMenu Menu;

        private UIMenuListItem lstHair; // Color Panel
        private UIMenuColorPanel pnlHairColorPrimary;
        private UIMenuColorPanel pnlHairColorSecondary;

        private UIMenuListItem lstEyebrows; // Color Panel + Opacity
        private UIMenuPercentagePanel pnlEyebrowOpacity;
        private UIMenuColorPanel pnlEyebrowColor;

        private UIMenuListItem lstFacialHair; // Male Only | Color Panel + Opacity
        private UIMenuPercentagePanel pnlFacialHairOpacity;
        private UIMenuColorPanel pnlFacialHairColor;

        private UIMenuListItem lstSkinBlemishes; // Opacity
        private UIMenuPercentagePanel pnlSkinBlemishesOpacity;

        private UIMenuListItem lstSkinAging; // Opacity
        private UIMenuPercentagePanel pnlSkinAgingOpacity;

        private UIMenuListItem lstSkinComplexion; // Opacity
        private UIMenuPercentagePanel pnlSkinComplexionOpacity;

        private UIMenuListItem lstSkinMoles; // Opacity
        private UIMenuPercentagePanel pnlSkinMolesOpacity;

        private UIMenuListItem lstSkinDamage; // Opacity
        private UIMenuPercentagePanel pnlSkinDamageOpacity;

        private UIMenuListItem lstEyeColor;

        private UIMenuListItem lstEyeMakeup; // Opacity
        private UIMenuPercentagePanel pnlEyeMakeupOpacity;
        private UIMenuColorPanel pnlEyeMakeupColor;

        private UIMenuListItem lstBlusher; // Female Only | Color Panel + Opacity
        private UIMenuPercentagePanel pnlBlusherOpacity;
        private UIMenuColorPanel pnlBlusherColor;

        private UIMenuListItem lstLipstick; // Color Panel + Opacity
        private UIMenuPercentagePanel pnlLipstickOpacity;
        private UIMenuColorPanel pnlLipstickColor;

        private List<dynamic> hairStyleList = new List<dynamic>();
        private List<dynamic> eyebrowsStyleList = new List<dynamic>();
        private List<dynamic> facialHairStylesList = new List<dynamic>();
        private List<dynamic> blemishesStyleList = new List<dynamic>();
        private List<dynamic> skinAgingStyleList = new List<dynamic>();
        private List<dynamic> complexionStyleList = new List<dynamic>();
        private List<dynamic> molesFrecklesStyleList = new List<dynamic>();
        private List<dynamic> skinDamageStyleList = new List<dynamic>();
        private List<dynamic> eyeColorList = new List<dynamic>();
        private List<dynamic> makeupStyleList = new List<dynamic>();
        private List<dynamic> blusherStyleList = new List<dynamic>();
        private List<dynamic> lipstickStyleList = new List<dynamic>();
        // Body
        private List<dynamic> chestHairStyleList = new List<dynamic>();
        private List<dynamic> bodyBlemishesList = new List<dynamic>();

        /* Source: vMenu
           0               Blemishes             0 - 23,   255  
           1               Facial Hair           0 - 28,   255  
           2               Eyebrows              0 - 33,   255  
           3               Ageing                0 - 14,   255  
           4               Makeup                0 - 74,   255  
           5               Blush                 0 - 6,    255  
           6               Complexion            0 - 11,   255  
           7               Sun Damage            0 - 10,   255  
           8               Lipstick              0 - 9,    255  
           9               Moles/Freckles        0 - 17,   255  
           10              Chest Hair            0 - 16,   255  
           11              Body Blemishes        0 - 11,   255  
           12              Add Body Blemishes    0 - 1,    255  

           */

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;
            menu.OnListChange += Menu_OnListChange;
            menu.OnIndexChange += Menu_OnIndexChange;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeBackward || state == MenuState.Closed)
                OnMenuClose(oldMenu);

            if (state == MenuState.ChangeForward)
                OnMenuOpen(newMenu);
        }

        private async void Menu_OnIndexChange(UIMenu menu, int newIndex)
        {
            if (
                menu.MenuItems[newIndex] == lstHair
                || menu.MenuItems[newIndex] == lstEyebrows
                || menu.MenuItems[newIndex] == lstFacialHair
                || menu.MenuItems[newIndex] == lstSkinBlemishes
                || menu.MenuItems[newIndex] == lstSkinAging
                || menu.MenuItems[newIndex] == lstSkinComplexion
                || menu.MenuItems[newIndex] == lstSkinMoles
                || menu.MenuItems[newIndex] == lstSkinDamage
                || menu.MenuItems[newIndex] == lstEyeColor
                || menu.MenuItems[newIndex] == lstEyeMakeup
                || menu.MenuItems[newIndex] == lstBlusher
                || menu.MenuItems[newIndex] == lstLipstick
                )
            {
                if (FaceCameraActive) return;
                FaceCameraActive = true;

                Cache.Player.CameraQueue.Reset();
                await Cache.Player.CameraQueue.View(new CameraBuilder()
                    .SkipTask()
                    .WithMotionBlur(0.5f)
                    .WithInterpolation(CreatorMenus.CameraViews[1], CreatorMenus.CameraViews[2], 500)
                );
            }
            else
            {
                if (!FaceCameraActive) return;
                FaceCameraActive = false;

                Cache.Player.CameraQueue.Reset();
                await Cache.Player.CameraQueue.View(new CameraBuilder()
                    .SkipTask()
                    .WithMotionBlur(0.5f)
                    .WithInterpolation(CreatorMenus.CameraViews[2], CreatorMenus.CameraViews[1], 500)
                );
            }
        }

        private void Menu_OnListChange(UIMenu menu, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == lstHair)
            {
                API.ClearPedFacialDecorations(Cache.PlayerPed.Handle);
                Cache.Character.Appearance.HairOverlay = new KeyValuePair<string, string>("", "");
                if (newIndex >= API.GetNumberOfPedDrawableVariations(Cache.PlayerPed.Handle, 2))
                {
                    API.SetPedComponentVariation(Cache.PlayerPed.Handle, 2, 0, 0, 0);
                    Cache.Character.Appearance.HairStyle = 0;
                }
                else
                {
                    API.SetPedComponentVariation(Cache.PlayerPed.Handle, 2, newIndex, 0, 0);
                    Cache.Character.Appearance.HairStyle = newIndex;
                    if (CharacterExtensions.HairOverlays.ContainsKey(newIndex))
                    {
                        KeyValuePair<string, string> overlay = CharacterExtensions.HairOverlays[newIndex];
                        API.SetPedFacialDecoration(Cache.PlayerPed.Handle, (uint)API.GetHashKey(overlay.Key), (uint)API.GetHashKey(overlay.Value));
                        Cache.Character.Appearance.HairOverlay = overlay;
                    }
                }

                UIMenuColorPanel primaryColor = (UIMenuColorPanel)listItem.Panels[0];
                UIMenuColorPanel secondaryColor = (UIMenuColorPanel)listItem.Panels[1];

                API.SetPedHairColor(Cache.PlayerPed.Handle, primaryColor.CurrentSelection, secondaryColor.CurrentSelection);
                Cache.Character.Appearance.HairPrimaryColor = primaryColor.CurrentSelection;
                Cache.Character.Appearance.HairSecondaryColor = secondaryColor.CurrentSelection;

                return;
            }

            if (listItem == lstEyebrows)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 2, newIndex, opacity);

                UIMenuColorPanel pnlColor = (UIMenuColorPanel)listItem.Panels[1];
                API.SetPedHeadOverlayColor(Cache.PlayerPed.Handle, 2, 1, pnlColor.CurrentSelection, pnlColor.CurrentSelection);

                Cache.Character.Appearance.Eyebrow = newIndex;
                Cache.Character.Appearance.EyebrowOpacity = opacity;
                Cache.Character.Appearance.EyebrowColor = pnlColor.CurrentSelection;

                return;
            }

            if (listItem == lstFacialHair && Cache.PlayerPed.Gender == Gender.Male)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 1, newIndex, opacity);

                UIMenuColorPanel pnlColor = (UIMenuColorPanel)listItem.Panels[1];
                API.SetPedHeadOverlayColor(Cache.PlayerPed.Handle, 1, 1, pnlColor.CurrentSelection, pnlColor.CurrentSelection);

                Cache.Character.Appearance.FacialHair = newIndex;
                Cache.Character.Appearance.FacialHairOpacity = opacity;
                Cache.Character.Appearance.FacialHairColor = pnlColor.CurrentSelection;

                return;
            }

            if (listItem == lstSkinBlemishes)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 0, newIndex, opacity);

                Cache.Character.Appearance.SkinBlemish = newIndex;
                Cache.Character.Appearance.SkinBlemishOpacity = opacity;

                return;
            }

            if (listItem == lstSkinAging)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 3, newIndex, opacity);

                Cache.Character.Appearance.SkinAging = newIndex;
                Cache.Character.Appearance.SkinAgingOpacity = opacity;

                return;
            }

            if (listItem == lstSkinComplexion)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 6, newIndex, opacity);

                Cache.Character.Appearance.SkinComplexion = newIndex;
                Cache.Character.Appearance.SkinComplexionOpacity = opacity;

                return;
            }

            if (listItem == lstSkinMoles)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 9, newIndex, opacity);

                Cache.Character.Appearance.SkinMoles = newIndex;
                Cache.Character.Appearance.SkinMolesOpacity = opacity;

                return;
            }

            if (listItem == lstSkinDamage)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 7, newIndex, opacity);

                Cache.Character.Appearance.SkinDamage = newIndex;
                Cache.Character.Appearance.SkinDamageOpacity = opacity;

                return;
            }

            if (listItem == lstEyeColor)
            {
                API.SetPedEyeColor(Cache.PlayerPed.Handle, newIndex);

                Cache.Character.Appearance.EyeColor = newIndex;

                return;
            }

            if (listItem == lstEyeMakeup)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                UIMenuColorPanel pnlColor = (UIMenuColorPanel)listItem.Panels[1];

                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 4, newIndex, opacity);
                API.SetPedHeadOverlayColor(Cache.PlayerPed.Handle, 4, 2, pnlColor.CurrentSelection, pnlColor.CurrentSelection);

                Cache.Character.Appearance.EyeMakeup = newIndex;
                Cache.Character.Appearance.EyeMakeupOpacity = opacity;
                Cache.Character.Appearance.EyeMakeupColor = pnlColor.CurrentSelection;

                return;
            }

            if (listItem == lstBlusher)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                UIMenuColorPanel pnlColor = (UIMenuColorPanel)listItem.Panels[1];

                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 5, newIndex, opacity);
                API.SetPedHeadOverlayColor(Cache.PlayerPed.Handle, 5, 2, pnlColor.CurrentSelection, pnlColor.CurrentSelection);

                Cache.Character.Appearance.Blusher = newIndex;
                Cache.Character.Appearance.BlusherOpacity = opacity;
                Cache.Character.Appearance.BlusherColor = pnlColor.CurrentSelection;

                return;
            }

            if (listItem == lstLipstick)
            {
                UIMenuPercentagePanel pnlOpacity = (UIMenuPercentagePanel)listItem.Panels[0];
                UIMenuColorPanel pnlColor = (UIMenuColorPanel)listItem.Panels[1];

                float opacity = pnlOpacity.Percentage;

                API.SetPedHeadOverlay(Cache.PlayerPed.Handle, 8, newIndex, opacity);
                API.SetPedHeadOverlayColor(Cache.PlayerPed.Handle, 8, 2, pnlColor.CurrentSelection, pnlColor.CurrentSelection);

                Cache.Character.Appearance.Lipstick = newIndex;
                Cache.Character.Appearance.LipstickOpacity = opacity;
                Cache.Character.Appearance.LipstickColor = pnlColor.CurrentSelection;

                return;
            }
        }

        private async void OnMenuClose(UIMenu menu)
        {
            PluginManager.Instance.DetachTickHandler(OnPlayerControls);

            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[2], CreatorMenus.CameraViews[1], 500)
            );
            menu.InstructionalButtons.Clear();
        }

        private async void OnMenuOpen(UIMenu menu)
        {
            PluginManager.Instance.AttachTickHandler(OnPlayerControls);

            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[1], CreatorMenus.CameraViews[2], 500)
            );
            FaceCameraActive = true;

            menu.Clear(); // clear the menu on load as the gender may of changed

            menu.InstructionalButtons.Clear();

            menu.InstructionalButtons.Add(CreatorMenus.btnRotateLeft);
            menu.InstructionalButtons.Add(CreatorMenus.btnRotateRight);

            CreateMenuItems();

            lstHair = new UIMenuListItem("Hair", hairStyleList, 0);
            lstHair.Description = $"Set your hair";
            pnlHairColorPrimary = new UIMenuColorPanel("1st Hair Color", UIMenuColorPanel.ColorPanelType.Hair);
            pnlHairColorSecondary = new UIMenuColorPanel("2nd Hair Color", UIMenuColorPanel.ColorPanelType.Hair);
            menu.AddItem(lstHair);
            lstHair.AddPanel(pnlHairColorPrimary);
            lstHair.AddPanel(pnlHairColorSecondary);

            lstEyebrows = new UIMenuListItem("Eyebrows", eyebrowsStyleList, 0);
            pnlEyebrowOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            pnlEyebrowColor = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Hair);
            menu.AddItem(lstEyebrows);
            lstEyebrows.AddPanel(pnlEyebrowOpacity);
            lstEyebrows.AddPanel(pnlEyebrowColor);

            if (Cache.PlayerPed.Gender == Gender.Male)
            {
                lstFacialHair = new UIMenuListItem("Facial Hair", facialHairStylesList, 0);
                pnlFacialHairOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
                pnlFacialHairColor = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Hair);
                menu.AddItem(lstFacialHair);
                lstFacialHair.AddPanel(pnlFacialHairOpacity);
                lstFacialHair.AddPanel(pnlFacialHairColor);
            }

            lstSkinBlemishes = new UIMenuListItem("Skin Blemishes", blemishesStyleList, 0);
            pnlSkinBlemishesOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            menu.AddItem(lstSkinBlemishes);
            lstSkinBlemishes.AddPanel(pnlSkinBlemishesOpacity);

            lstSkinAging = new UIMenuListItem("Skin Aging", skinAgingStyleList, 0);
            pnlSkinAgingOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            menu.AddItem(lstSkinAging);
            lstSkinAging.AddPanel(pnlSkinAgingOpacity);

            lstSkinComplexion = new UIMenuListItem("Skin Complexion", complexionStyleList, 0);
            pnlSkinComplexionOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            menu.AddItem(lstSkinComplexion);
            lstSkinComplexion.AddPanel(pnlSkinComplexionOpacity);

            lstSkinMoles = new UIMenuListItem("Moles & Freckles", molesFrecklesStyleList, 0);
            pnlSkinMolesOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            menu.AddItem(lstSkinMoles);
            lstSkinMoles.AddPanel(pnlSkinMolesOpacity);

            lstSkinDamage = new UIMenuListItem("Skin Damage", skinDamageStyleList, 0);
            pnlSkinDamageOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            menu.AddItem(lstSkinDamage);
            lstSkinDamage.AddPanel(pnlSkinDamageOpacity);

            lstEyeColor = new UIMenuListItem("Eye Color", eyeColorList, 0);
            menu.AddItem(lstEyeColor);

            lstEyeMakeup = new UIMenuListItem("Makeup", makeupStyleList, 0);
            pnlEyeMakeupOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            pnlEyeMakeupColor = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Makeup);
            menu.AddItem(lstEyeMakeup);
            lstEyeMakeup.AddPanel(pnlEyeMakeupOpacity);
            lstEyeMakeup.AddPanel(pnlEyeMakeupColor);

            if (Cache.PlayerPed.Gender == Gender.Female)
            {
                lstBlusher = new UIMenuListItem("Blusher", blusherStyleList, 0);
                pnlBlusherOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
                pnlBlusherColor = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Makeup);
                menu.AddItem(lstBlusher);
                lstBlusher.AddPanel(pnlBlusherOpacity);
                lstBlusher.AddPanel(pnlBlusherColor);
            }

            lstLipstick = new UIMenuListItem("Lipstick", lipstickStyleList, 0);
            pnlLipstickOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            pnlLipstickColor = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Makeup);
            menu.AddItem(lstLipstick);
            lstLipstick.AddPanel(pnlLipstickOpacity);
            lstLipstick.AddPanel(pnlLipstickColor);
        }

        private async Task OnPlayerControls()
        {
            CreatorMenus._MenuPool.ProcessMouse();

            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Cache.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Cache.PlayerPed.Heading -= 10f;
            }
        }

        private void CreateMenuItems()
        {
            hairStyleList.Clear();
            int maxHairStyles = API.GetNumberOfPedDrawableVariations(Cache.PlayerPed.Handle, 2);
            for (int i = 0; i < maxHairStyles; i++)
            {
                hairStyleList.Add($"Style #{i + 1}");
            }
            hairStyleList.Add($"Style #{maxHairStyles + 1}");

            blemishesStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(0); i++)
            {
                blemishesStyleList.Add($"Style #{i + 1}");
            }

            facialHairStylesList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(1); i++)
            {
                facialHairStylesList.Add($"Style #{i + 1}");
            }

            eyebrowsStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(2); i++)
            {
                eyebrowsStyleList.Add($"Style #{i + 1}");
            }

            skinAgingStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(3); i++)
            {
                skinAgingStyleList.Add($"Style #{i + 1}");
            }

            makeupStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(4); i++)
            {
                makeupStyleList.Add($"Style #{i + 1}");
            }

            blusherStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(5); i++)
            {
                blusherStyleList.Add($"Style #{i + 1}");
            }

            complexionStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(6); i++)
            {
                complexionStyleList.Add($"Style #{i + 1}");
            }

            skinDamageStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(7); i++)
            {
                skinDamageStyleList.Add($"Style #{i + 1}");
            }

            lipstickStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(8); i++)
            {
                lipstickStyleList.Add($"Style #{i + 1}");
            }

            molesFrecklesStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(9); i++)
            {
                molesFrecklesStyleList.Add($"Style #{i + 1}");
            }

            chestHairStyleList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(10); i++)
            {
                chestHairStyleList.Add($"Style #{i + 1}");
            }

            bodyBlemishesList.Clear();
            for (int i = 0; i < API.GetNumHeadOverlayValues(11); i++)
            {
                bodyBlemishesList.Add($"Style #{i + 1}");
            }

            eyeColorList.Clear();
            for (int i = 0; i < 32; i++)
            {
                eyeColorList.Add($"Eye Color #{i + 1}");
            }
        }
    }
}
