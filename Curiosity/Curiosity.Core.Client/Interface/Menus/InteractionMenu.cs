using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Managers;
using NativeUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus
{
    public class InteractionMenu : Manager<InteractionMenu>
    {
        public static MenuPool _MenuPool;
        private UIMenu menuMain;

        // menu items
        private UIMenuListItem mlGpsLocations;
        private List<dynamic> gpsLocations = new List<dynamic>();
        private int gpsIndex = 0;

        public override void Begin()
        {
            _MenuPool = new MenuPool();
            _MenuPool.MouseEdgeEnabled = false;

            menuMain = new UIMenu("Interaction Menu", "Player Interactions");
            _MenuPool.Add(menuMain);

            menuMain.OnMenuClose += MenuMain_OnMenuClose;
            menuMain.OnMenuOpen += MenuMain_OnMenuOpen;
            menuMain.OnListChange += MenuMain_OnListChange;
            menuMain.OnListSelect += MenuMain_OnListSelect;
        }

        private void MenuMain_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == mlGpsLocations)
            {
                gpsIndex = newIndex;
                var position = listItem.Items[newIndex];
            }
        }

        private void MenuMain_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == mlGpsLocations)
            {
                gpsIndex = newIndex;
            }
        }

        private void MenuMain_OnMenuOpen(UIMenu sender)
        {
            mlGpsLocations = new UIMenuListItem("GPS", gpsLocations, gpsIndex);
        }

        private void MenuMain_OnMenuClose(UIMenu sender)
        {
            Instance.DetachTickHandler(OnMenuDisplay);
        }

        private async Task OnMenuDisplay()
        {
            _MenuPool.ProcessMenus();
            _MenuPool.ProcessMouse();
        }

        [TickHandler(SessionWait = true)]
        private async Task OnMenuControls()
        {
            if (Cache.Character.MarkedAsRegistered && API.NetworkIsSessionActive() && (Game.PlayerPed.IsAlive || Cache.Player.User.IsStaff))
            {
                if (Game.IsControlJustPressed(0, Control.InteractionMenu))
                {
                    Instance.AttachTickHandler(OnMenuDisplay);
                    menuMain.Visible = true;
                }
            }
        }
    }
}
