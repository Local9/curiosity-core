﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Utils;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    internal class PoliceMenu
    {
        private UIMenu menu;
        EventSystem EventSystem => EventSystem.GetModule();

        public UIMenu CreateMenu(UIMenu m)
        {
            menu = m;



            return menu;
        }

        public void Init()
        {
            PluginManager.Instance.AttachTickHandler(CheckControls);
        }

        public void Dispose()
        {
            PluginManager.Instance.DetachTickHandler(CheckControls);
        }

        private async Task CheckControls()
        {
            if (!Game.IsPaused
                && !IsPauseMenuRestarting()
                && IsScreenFadedIn()
                && !IsPlayerSwitchInProgress()
                && Cache.Character.MarkedAsRegistered)
            {
                if (InteractionMenu.MenuPool.IsAnyMenuOpen() && menu.Visible)
                {
                    if (ControlHelper.IsControlJustPressed(Control.Context, true, ControlModifier.Alt))
                    {
                        menu.Visible = false;
                        InteractionMenu.MenuPool.CloseAllMenus();
                    }
                }
                else if (!menu.Visible)
                {
                    if (ControlHelper.IsControlJustPressed(Control.Context, true, ControlModifier.Alt))
                    {
                        menu.Visible = true;
                    }
                }
            }
        }
    }
}
