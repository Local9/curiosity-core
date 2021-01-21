using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus
{
    public class InteractionMenu : Manager<InteractionMenu>
    {
        public static MenuPool MenuPool;
        private UIMenu menuMain;

        // menu items
        private List<dynamic> gpsLocations = new List<dynamic>();
        private int gpsIndex = 0;
        private UIMenuListItem mlGpsLocations;

        public override void Begin()
        {
            MenuPool = new MenuPool();
            MenuPool.MouseEdgeEnabled = false;

            menuMain = new UIMenu("Interaction Menu", "Player Interactions");
            menuMain.MouseControlsEnabled = false;
            MenuPool.Add(menuMain);

            menuMain.OnMenuClose += MenuMain_OnMenuClose;
            menuMain.OnMenuOpen += MenuMain_OnMenuOpen;
            menuMain.OnListChange += MenuMain_OnListChange;
            menuMain.OnListSelect += MenuMain_OnListSelect;

            API.RegisterKeyMapping("open_interaction_menu", "Open Interactive Menu", "keyboard", "M");
            API.RegisterCommand("open_interaction_menu", new Action(OpenMenuCommand), false);
        }

        private void OpenMenuCommand()
        {
            if (!MenuPool.IsAnyMenuOpen() && Cache.Character.MarkedAsRegistered && API.NetworkIsSessionActive() && (Game.PlayerPed.IsAlive || Cache.Player.User.IsStaff))
            {
                if (Game.IsControlJustPressed(0, Control.InteractionMenu))
                {
                    Instance.AttachTickHandler(OnMenuDisplay);
                    menuMain.Visible = !menuMain.Visible;
                }
            }
        }

        private Vector3 FindClosestPoint(Vector3 startingPoint, IEnumerable<Vector3> points)
        {
            if (points.Count() == 0) return Vector3.Zero;

            return points.OrderBy(x => Vector3.Distance(startingPoint, x)).First();
        }

        private void MenuMain_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == mlGpsLocations)
            {
                gpsIndex = newIndex;
                string key = (string)listItem.Items[newIndex];

                Logger.Debug($"Selected Key: {key}");

                List<Position> positions = BlipManager.ManagerInstance.Locations[key];
                List<Vector3> posVectors = new List<Vector3>();
                positions.ForEach(x => posVectors.Add(x.AsVector()));

                Vector3 closestPosition = FindClosestPoint(Game.PlayerPed.Position, posVectors);

                if (closestPosition.Equals(Vector3.Zero)) return;

                API.SetNewWaypoint(closestPosition.X, closestPosition.Y);
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
            AddGpsMenuItem();

            // Add Sub Menus
        }

        private void MenuMain_OnMenuClose(UIMenu sender)
        {
            DisposeMenu();
        }

        public void DisposeMenu()
        {
            Instance.DetachTickHandler(OnMenuDisplay);
            menuMain.Clear(); // RESET
        }

        private async Task OnMenuDisplay()
        {
            MenuPool.ProcessMenus();
            // MenuPool.ProcessMouse();
            MenuPool.MouseEdgeEnabled = false;
        }

        // Menu Items
        private void AddGpsMenuItem()
        {
            gpsLocations.Clear();

            foreach (KeyValuePair<string, List<Position>> kvp in BlipManager.ManagerInstance.Locations)
            {
                if (!gpsLocations.Contains(kvp.Key))
                    gpsLocations.Add(kvp.Key);
            }

            if (gpsLocations.Count > 1)
                gpsLocations.Sort((x, y) => string.Compare(x, y));

            mlGpsLocations = new UIMenuListItem("GPS", gpsLocations, gpsIndex);

            menuMain.AddItem(mlGpsLocations);
        }
    }
}
