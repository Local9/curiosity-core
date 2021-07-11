using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Environment.Entities.Models;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    enum FaceFeature
    {
        NoseWidth, // C
        NoseTipHeight, // c
        NosePeakLength,
        NoseBoneHeight, // Shallow or Roman nose
        NoseTipTurned, // c
        NoseBoneTwist, // Broken nose/curve
        EyeBrowHeight, // C
        EyeBrowProtrusion, // C
        CheeksBoneHeight,
        CheeksBoneWidth,
        CheeksWidth,
        EyesOpenning, // C
        LipsThickness,
        JawBoneWidth,
        JawBoneBackLength,
        ChinBoneLowering,
        ChinBoneLength,
        ChinBoneWidth,
        ChinHole,
        NeckThickness,
    }

    //Items: U, D, L, R
    //Brow - Up, Down, In, Out (Standard, Protruding, Shallow)
    //Eyes - Slider(Squint, Wide) (Standard, Squint, Wide)
    //Nose - Up, Down, Narrow, Wide (Standard, large, small)
    //Nose Profile - Crooked, Curved, Short, Long (Standard, Crooked, Curved)
    //Nose Tip - Up, down, left, right (Standard, Broken, Up Turned)
    //Cheekbones - Up, Down, In, Out (Standard, Large, Small)
    //Cheeks - Slider(Gaunt, Puffed) (Standard, Fat, Thin)
    //Lips - Slider(thin, fat) (Standard, Large, Small)
    //Jaw - Round, Squad, Narrow, Wide (Standard, Squard, Round)
    //Chin Profile - Up, Down, In, Out (Standard, Large, Small)
    //Chin Shape - Rounded, Bum, Square, Pointed (Standard, Pointed, Round)

    class CharacterFeatures
    {
        private const float STANDARD = .5f;

        // Eye Brow
        UIMenuListItem lstBrow; // t
        UIMenuListItem lstEyes; // t
        UIMenuListItem lstNoseThickness; // t
        UIMenuListItem lstNoseProfile; // t
        UIMenuListItem lstNoseTip; // t
        UIMenuListItem lstCheeks;
        UIMenuListItem lstCheekBones;
        UIMenuListItem lstLips;
        UIMenuListItem lstJaw;
        UIMenuListItem lstChinProfile;
        UIMenuListItem lstChinShape;
        // Panels
        UIMenuGridPanel gridBrow;
        UIMenuHorizontalOneLineGridPanel gridEyes;
        UIMenuHorizontalOneLineGridPanel gridNose;
        UIMenuGridPanel gridNoseProfile;
        UIMenuGridPanel gridNoseTip;
        UIMenuHorizontalOneLineGridPanel gridCheeks;
        UIMenuGridPanel gridCheekBones;
        UIMenuHorizontalOneLineGridPanel gridLips;
        UIMenuGridPanel gridJaw;
        UIMenuGridPanel gridChinProfile;
        UIMenuGridPanel gridChinShape;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;
            menu.OnListChange += Menu_OnListChange;

            menu.InstructionalButtons.Add(CreatorMenus.btnRotateLeft);
            menu.InstructionalButtons.Add(CreatorMenus.btnRotateRight);

            lstBrow = new UIMenuListItem("Brow", new List<dynamic>() { "Standard" }, 0);
            lstBrow.Description = lstBrow.Text;
            menu.AddItem(lstBrow);
            lstEyes = new UIMenuListItem("Eyes", new List<dynamic>() { "Standard" }, 0);
            lstEyes.Description = lstEyes.Text;
            menu.AddItem(lstEyes);
            lstNoseThickness = new UIMenuListItem("Nose", new List<dynamic>() { "Standard" }, 0);
            lstNoseThickness.Description = lstNoseThickness.Text;
            menu.AddItem(lstNoseThickness);
            lstNoseProfile = new UIMenuListItem("Nose Profile", new List<dynamic>() { "Standard" }, 0);
            lstNoseProfile.Description = lstNoseProfile.Text;
            menu.AddItem(lstNoseProfile);
            lstNoseTip = new UIMenuListItem("Nose Tip", new List<dynamic>() { "Standard" }, 0);
            lstNoseTip.Description = lstNoseTip.Text;
            menu.AddItem(lstNoseTip);
            lstCheeks = new UIMenuListItem("Cheeks", new List<dynamic>() { "Standard" }, 0);
            lstCheeks.Description = lstCheeks.Text;
            menu.AddItem(lstCheeks);
            lstCheekBones = new UIMenuListItem("Cheek Bones", new List<dynamic>() { "Standard" }, 0);
            lstCheekBones.Description = lstCheekBones.Text;
            menu.AddItem(lstCheekBones);
            lstLips = new UIMenuListItem("Lips", new List<dynamic>() { "Standard" }, 0);
            lstLips.Description = lstLips.Text;
            menu.AddItem(lstLips);
            lstJaw = new UIMenuListItem("Jaw", new List<dynamic>() { "Standard" }, 0);
            lstJaw.Description = lstJaw.Text;
            menu.AddItem(lstJaw);
            lstChinProfile = new UIMenuListItem("Chin Profile", new List<dynamic>() { "Standard" }, 0);
            lstChinProfile.Description = lstChinProfile.Text;
            menu.AddItem(lstChinProfile);
            lstChinShape = new UIMenuListItem("Chin Shape", new List<dynamic>() { "Standard" }, 0);
            lstChinShape.Description = lstChinShape.Text;
            menu.AddItem(lstChinShape);


            gridBrow = new UIMenuGridPanel("Up", "In", "Out", "Down", new PointF(STANDARD, STANDARD));
            lstBrow.AddPanel(gridBrow);
            gridEyes = new UIMenuHorizontalOneLineGridPanel("Wide", "Squint");
            lstEyes.AddPanel(gridEyes);
            gridNose = new UIMenuHorizontalOneLineGridPanel("Small", "Large");
            lstNoseThickness.AddPanel(gridNose);
            gridNoseProfile = new UIMenuGridPanel("Crooked", "Left", "Right", "Curved", new PointF(STANDARD, STANDARD));
            lstNoseProfile.AddPanel(gridNoseProfile);
            gridNoseTip = new UIMenuGridPanel("Up", "T-Up", "T-Down", "Down", new PointF(STANDARD, STANDARD));
            lstNoseTip.AddPanel(gridNoseTip);
            gridCheekBones = new UIMenuGridPanel("Up", "In", "Out", "Down", new PointF(STANDARD, STANDARD));
            lstCheekBones.AddPanel(gridCheekBones);
            gridCheeks = new UIMenuHorizontalOneLineGridPanel("Puffed", "Gaunt");
            lstCheeks.AddPanel(gridCheeks);
            gridLips = new UIMenuHorizontalOneLineGridPanel("Fat", "Thin");
            lstLips.AddPanel(gridLips);
            gridJaw = new UIMenuGridPanel("Round", "Narrow", "Square", "Wide", new PointF(STANDARD, STANDARD));
            lstJaw.AddPanel(gridJaw);
            gridChinProfile = new UIMenuGridPanel("Up", "In", "Out", "Down", new PointF(STANDARD, STANDARD));
            lstChinProfile.AddPanel(gridChinProfile);
            gridChinShape = new UIMenuGridPanel("Rounded", "Bum", "Square", "Pointed", new PointF(STANDARD, STANDARD));
            lstChinShape.AddPanel(gridChinShape);

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeBackward)
                OnMenuClose();

            if (state == MenuState.ChangeForward)
                OnMenuOpen();
        }

        private void Menu_OnListChange(UIMenu menu, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == lstBrow)
            {
                float y;
                float x;

                UIMenuGridPanel panel = (UIMenuGridPanel)listItem.Panels[0];

                y = Project(panel.CirclePosition.Y, 0.5f, 1.0f);
                x = Project(panel.CirclePosition.X, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.EyeBrowProtrusion, x);
                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.EyeBrowHeight, y);

                Cache.Character.ChangeFeature((int)FaceFeature.EyeBrowProtrusion, x);
                Cache.Character.ChangeFeature((int)FaceFeature.EyeBrowHeight, y);
            }

            if (listItem == lstEyes)
            {
                float x;

                UIMenuHorizontalOneLineGridPanel panel = (UIMenuHorizontalOneLineGridPanel)listItem.Panels[0];

                x = panel.CirclePosition.X;
                x = Project(x, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.EyesOpenning, x);
                Cache.Character.ChangeFeature((int)FaceFeature.EyesOpenning, x);
            }

            if (listItem == lstNoseThickness)
            {
                float x;

                UIMenuHorizontalOneLineGridPanel panel = (UIMenuHorizontalOneLineGridPanel)listItem.Panels[0];

                x = panel.CirclePosition.X;
                x = Project(x, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.NoseWidth, x);
                Cache.Character.ChangeFeature((int)FaceFeature.NoseWidth, x);
            }

            if (listItem == lstNoseProfile)
            {
                float x;
                float y;

                UIMenuGridPanel panel = (UIMenuGridPanel)listItem.Panels[0];

                y = Project(panel.CirclePosition.Y, 0.5f, 1.0f);
                x = Project(panel.CirclePosition.X, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.NoseBoneHeight, y);
                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.NoseBoneTwist, x);

                Cache.Character.ChangeFeature((int)FaceFeature.NoseBoneHeight, y);
                Cache.Character.ChangeFeature((int)FaceFeature.NoseBoneTwist, x);
            }

            if (listItem == lstNoseTip)
            {
                float x;
                float y;

                UIMenuGridPanel panel = (UIMenuGridPanel)listItem.Panels[0];

                y = Project(panel.CirclePosition.Y, 0.5f, 1.0f);
                x = Project(panel.CirclePosition.X, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.NoseTipHeight, y);
                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.NoseTipTurned, x);

                Cache.Character.ChangeFeature((int)FaceFeature.NoseTipHeight, y);
                Cache.Character.ChangeFeature((int)FaceFeature.NoseTipTurned, x);
            }

            if (listItem == lstCheeks)
            {
                float x;

                UIMenuHorizontalOneLineGridPanel panel = (UIMenuHorizontalOneLineGridPanel)listItem.Panels[0];

                x = panel.CirclePosition.X;
                x = Project(x, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.CheeksWidth, x);
                Cache.Character.ChangeFeature((int)FaceFeature.CheeksWidth, x);
            }

            if (listItem == lstCheekBones)
            {
                float x;
                float y;

                UIMenuGridPanel panel = (UIMenuGridPanel)listItem.Panels[0];

                y = Project(panel.CirclePosition.Y, 0.5f, 1.0f);
                x = Project(panel.CirclePosition.X, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.CheeksBoneHeight, y);
                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.CheeksBoneWidth, x);

                Cache.Character.ChangeFeature((int)FaceFeature.CheeksBoneHeight, y);
                Cache.Character.ChangeFeature((int)FaceFeature.CheeksBoneWidth, x);
            }

            if (listItem == lstLips)
            {
                float x;

                UIMenuHorizontalOneLineGridPanel panel = (UIMenuHorizontalOneLineGridPanel)listItem.Panels[0];

                x = panel.CirclePosition.X;
                x = Project(x, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.LipsThickness, x);
                Cache.Character.ChangeFeature((int)FaceFeature.LipsThickness, x);
            }

            if (listItem == lstJaw)
            {
                float x;
                float y;

                UIMenuGridPanel panel = (UIMenuGridPanel)listItem.Panels[0];

                y = Project(panel.CirclePosition.Y, 0.5f, 1.0f);
                x = Project(panel.CirclePosition.X, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.JawBoneBackLength, y);
                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.JawBoneWidth, x);

                Cache.Character.ChangeFeature((int)FaceFeature.JawBoneBackLength, y);
                Cache.Character.ChangeFeature((int)FaceFeature.JawBoneWidth, x);
            }

            if (listItem == lstChinProfile)
            {
                float x;
                float y;

                UIMenuGridPanel panel = (UIMenuGridPanel)listItem.Panels[0];

                y = Project(panel.CirclePosition.Y, 0.5f, 1.0f);
                x = Project(panel.CirclePosition.X, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.ChinBoneLowering, y);
                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.ChinBoneLength, x);

                Cache.Character.ChangeFeature((int)FaceFeature.ChinBoneLowering, y);
                Cache.Character.ChangeFeature((int)FaceFeature.ChinBoneLength, x);
            }

            if (listItem == lstChinShape)
            {
                float x;
                float y;

                UIMenuGridPanel panel = (UIMenuGridPanel)listItem.Panels[0];

                y = Project(panel.CirclePosition.Y, 0.5f, 1.0f);
                x = Project(panel.CirclePosition.X, 0.5f, 1.0f);

                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.ChinBoneWidth, y);
                API.SetPedFaceFeature(Cache.Entity.Id, (int)FaceFeature.ChinHole, x);

                Cache.Character.ChangeFeature((int)FaceFeature.ChinBoneWidth, y);
                Cache.Character.ChangeFeature((int)FaceFeature.ChinHole, x);
            }
        }

        private async void OnMenuClose()
        {
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[2], CreatorMenus.CameraViews[1], 500)
            );
            PluginManager.Instance.DetachTickHandler(OnPlayerControls);
        }

        private async void OnMenuOpen()
        {
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[1], CreatorMenus.CameraViews[2], 500)
            );
            PluginManager.Instance.AttachTickHandler(OnPlayerControls);
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

        float Project(float value, float start, float end)
        {
            if (start == end) throw new ArgumentException("Start and end must not be equal.");
            return (value - start) / (end - start);
        }
    }
}
