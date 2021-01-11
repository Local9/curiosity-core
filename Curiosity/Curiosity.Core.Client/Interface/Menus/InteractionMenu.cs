﻿using CitizenFX.Core;
using Curiosity.Core.Client.Managers;
using NativeUI;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus
{
    public class InteractionMenu : Manager<InteractionMenu>
    {
        public static MenuPool _MenuPool;
        private UIMenu menuMain;

        public override void Begin()
        {
            _MenuPool = new MenuPool();
            _MenuPool.MouseEdgeEnabled = false;

            menuMain = new UIMenu("Interaction Menu", "Player Interactions");
            _MenuPool.Add(menuMain);

            menuMain.OnMenuClose += MenuMain_OnMenuClose;
        }

        private void MenuMain_OnMenuClose(UIMenu sender)
        {
            PluginManager.Instance.DetachTickHandler(OnMenuDisplay);
        }

        private async Task OnMenuDisplay()
        {
            _MenuPool.ProcessMenus();
            _MenuPool.ProcessMouse();
        }

        [TickHandler(SessionWait = true)]
        private async Task OnMenuControls()
        {
            if (Cache.Character.MarkedAsRegistered && Game.PlayerPed.IsAlive)
            {
                if (Game.IsControlJustPressed(0, Control.InteractionMenu))
                {
                    PluginManager.Instance.AttachTickHandler(OnMenuDisplay);
                    menuMain.Visible = true;
                }
            }
        }
    }
}