﻿using System;

namespace Curiosity.Client.net.Classes.Menus.CharacterEditor.CustomizeMenu
{
    class MPOverlayButton : MenuItemSubMenu
    {
        public MPOverlayButton(CharacterEditorMenu root)
        {
            Title = "Overlays";
            SubMenu = new MPOverlayMenu(root);
            //OnSelect = new Action<MenuItem>((menuItem) => { Roleplay.Client.CharacterEditor.LookAtFace(); });
            OnSelect = new Action<MenuItem>((menuItem) => { });
        }

        public override void Refresh()
        {
            base.Refresh();
            SubMenu.Refresh();
        }

        public override void OnTick(long frameCount, int frameTime, long gameTimer)
        {
            base.OnTick(frameCount, frameTime, gameTimer);
            SubMenu.OnTick(frameCount, frameTime, gameTimer);
        }
    }
}
