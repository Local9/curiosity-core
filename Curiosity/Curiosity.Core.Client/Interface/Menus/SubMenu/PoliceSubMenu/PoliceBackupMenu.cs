using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Models;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.PoliceSubMenu
{
    internal class PoliceBackupMenu
    {
        bool isCurrentlyAssisting = false;

        private EventSystem eventSystem => EventSystem.GetModule();
        UIMenu _menu;

        UIMenuItem miClearCurrent = new UIMenuItem("Clear Current Assist");

        Dictionary<UIMenuItem, int> playersRequestingBackup = new();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            _menu = menu;
            return _menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward || state == MenuState.Opened)
                OnMenuOpen();
        }

        private async void OnMenuOpen()
        {
            _menu.Clear();

            _menu.AddItem(miClearCurrent);

            List<GenericUserListItem> list = await eventSystem.Request<List<GenericUserListItem>>("mission:assistance:list");

            Logger.Debug($"{list.Count} requesting assistance");

            foreach (GenericUserListItem item in list)
            {
                UIMenuItem uIMenuItem = new UIMenuItem(item.Name, $"Assist {item.Name}");
                uIMenuItem.ItemData = item;
                _menu.AddItem(uIMenuItem);
            }
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (isCurrentlyAssisting)
            {
                if (selectedItem == miClearCurrent)
                {
                    isCurrentlyAssisting = false;
                    Notify.Info($"Cleared current assist.");
                    return;
                }

                Notify.Alert("Currently on an active assist, please complete or clear it.");
                return;
            }
            isCurrentlyAssisting = true;

            GenericUserListItem item = selectedItem.ItemData;
            Logger.Debug($"Responding to {item.Name}");

            Position pos = await eventSystem.Request<Position>("mission:assistance:accept", item.ServerId);
            Logger.Debug($"Location {pos}");

            Vector3 v3 = pos.AsVector();

            Blip blip = World.CreateBlip(v3);
            blip.Sprite = (BlipSprite)767;
            blip.IsFlashing = true;
            blip.Color = BlipColor.Red;
            blip.ShowRoute = true;

            while (!Game.PlayerPed.IsInRangeOf(v3, 100f) && isCurrentlyAssisting)
            {
                await BaseScript.Delay(250);
            }

            if (blip.Exists())
                blip.Delete();

            isCurrentlyAssisting = false;
        }
    }
}
